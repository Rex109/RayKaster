using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using zlib;

namespace RayKaster
{
    public partial class Form1 : Form
    {
        const String IW3_SIGNATURE = "SVdmZnUxMDAFAAAA";
        String file; 

        public Form1()
        {
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            file = openFileDialog1.FileName;

            try
            {
                String header = Convert.ToBase64String(File.ReadAllBytes(file).Take(12).ToArray());

                if (header != IW3_SIGNATURE)
                {
                    MessageBox.Show("Invalid map header", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured while loading a file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            textBox1.Text = file;
            textBox2.Text = Path.GetFileNameWithoutExtension(file);
            textBox3.Text = "";
            textBox3.MaxLength = textBox2.Text.Length;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            byte[] bytes = File.ReadAllBytes(file).Skip(12).ToArray();

            String uncompressed = DecompressString(bytes);

            File.WriteAllBytes(Path.GetDirectoryName(file) + "\\" + textBox3.Text + ".ff", Combine(Convert.FromBase64String(IW3_SIGNATURE), CompressString(uncompressed.Replace(textBox2.Text+".d3dbsp", textBox3.Text+".d3dbsp").Replace(textBox2.Text + ".gsc", textBox3.Text + ".gsc"))));

            button1.Enabled = true;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public static string DecompressString(byte[] buffer)
        {
            MemoryStream memOutput = new MemoryStream();
            ZOutputStream zipOut = new ZOutputStream(memOutput);

            zipOut.Write(buffer, 0, buffer.Length);
            zipOut.finish();

            memOutput.Seek(0, SeekOrigin.Begin);
            byte[] result = memOutput.ToArray();

            var str = System.Text.Encoding.Default.GetString(result);

            return str;
        }

        public static byte[] CompressString(string source)
        {
            byte[] buffer = System.Text.Encoding.Default.GetBytes(source);

            MemoryStream memOutput = new MemoryStream();
            ZOutputStream zipOut = new ZOutputStream(memOutput, zlibConst.Z_BEST_SPEED);

            zipOut.Write(buffer, 0, buffer.Length);
            zipOut.finish();

            memOutput.Seek(0, SeekOrigin.Begin);
            byte[] result = memOutput.ToArray();

            return result;
        }
    }

}

