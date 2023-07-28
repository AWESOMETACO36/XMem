using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JRPC_Client;
using XDevkit;

namespace XMem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        IXboxConsole xbox;
        Dictionary<String, byte[]> foundPatterns = new Dictionary<String, byte[]>(); // stores any found patterns to be cross referenced when rescanned for changes


        private void button1_Click(object sender, EventArgs e)
        {
            xbox.Connect(out xbox);
            if (xbox.Connect(out xbox) == true)
            {
                xbox.XNotify("Tool Connected");
                MessageBox.Show("Tool Sucessfully Connected");
            }
            else
            {
                MessageBox.Show("Tool Failed to Connect!");
            }
        }

        private void dumpMemory_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(() =>
            {
                dumpMem("memory_dump");
            });
            t.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /* uint offsetPos = 0x00000000;

             for (int i = 0; i < 1000; i++)
             {
                 offsetPos += 0x00000010; // Increment by 0x10 (16 in decimal)
                 Console.WriteLine(offsetPos.ToString("X"));
             }*/

            int chunkSize = 500;
            byte[] pattern = BitConverter.GetBytes(int.Parse(textBox2.Text));
            Console.WriteLine("Pattern searched for: ");

            foreach (byte b in pattern)
            {
                Console.WriteLine($"{b:X2}");
            }

            debugTextbox.Invoke(new MethodInvoker(delegate { debugTextbox.AppendText($"Started pattern scan for {textBox2.Text}\n"); }));

            string fileName = "C:\\Projects\\C#\\XMem\\memory_dump.bin";

            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[chunkSize];
                int bytesRead;

                long offset = 0;
                while ((bytesRead = stream.Read(buffer, 0, chunkSize)) > 0)
                {
                    // Check if the pattern is present in the current chunk
                    int patternOffset = FindPattern(buffer, pattern);
                    if (patternOffset != -1)
                    {
                        long absoluteOffset = offset + patternOffset; // gets the index for the correct offset


                        outputTextbox.Invoke(new MethodInvoker(delegate { outputTextbox.AppendText($"Pattern found at offset 0x{absoluteOffset:X}\n"); }));

                        String offsett = absoluteOffset.ToString("X");
                        foundPatterns.Add(offsett, buffer);
                    }

                    offset += bytesRead;
                }
            }




            // put all found offsets in dictionary (keys are the offsets, their values are their byte chunks)

            // on rescan, for every item in the dictionary iterate to its key and cross check its value for changes









        }


        // iteratively checks if the given by array is contained in the given source byte array
        public static int FindPattern(byte[] source, byte[] pattern)
        {
            if (source == null || pattern == null || source.Length == 0 || pattern.Length == 0 || pattern.Length > source.Length)
                return -1;

            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return i;
            }

            return -1; // Pattern not found
        }

   

        private void button1_Click_1(object sender, EventArgs e)
        {

            Thread t = new Thread(() =>
            {

                // redump memory, pattern scan for new value, if any offsets are found that match any previously found offset output it

                 dumpMem("memory_dump2"); // dumps a secondary updated file to compare

                int chunkSize = 500;

                byte[] pattern = BitConverter.GetBytes(int.Parse(textBox2.Text));

                debugTextbox.Invoke(new MethodInvoker(delegate { debugTextbox.AppendText($"Started pattern scan for {textBox2.Text}\n"); }));

                string fileName = "C:\\Projects\\C#\\XMem\\memory_dump2.bin";

                using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[chunkSize] ;
                    int bytesRead;

                    long offset = 0;
                    while ((bytesRead = stream.Read(buffer, 0, chunkSize)) > 0)
                    {
                        int patternOffset = FindPattern(buffer, pattern);
                        if (patternOffset != -1)
                        {
                            long absoluteOffset = offset + patternOffset;

                            foreach (var kp in foundPatterns)
                            {
                                string offsett = kp.Key;

                                if (offsett.Equals(absoluteOffset.ToString("X")) || offsett.Contains(absoluteOffset.ToString("X")))
                                {
                                    outputTextbox.Invoke(new MethodInvoker(delegate { outputTextbox.AppendText($"Change detected at offset {offsett}\n"); }));
                                }
                            }
                            Console.WriteLine($"Pattern found for {textBox2.Text} @ {offset.ToString("X")}");
                            
                        }
                        offset += bytesRead;
                    }

                    int count = 0;
                    foreach (var kp in foundPatterns)
                    {
                        count++;
                        string offsett = kp.Key;

                        Console.WriteLine($"old list offset #{count}: {offsett}");
                    }
           
                    debugTextbox.Invoke(new MethodInvoker(delegate { debugTextbox.AppendText($"Re-Scan for changes concluded...\n"); }));
                    stream.Close();
                }
            });

            t.Start();


        }

        private void dumpMem(String filename)
        {

            debugTextbox.Invoke(new MethodInvoker(delegate { outputTextbox.Clear(); }));
            debugTextbox.Invoke(new MethodInvoker(delegate { debugTextbox.Clear(); }));
            debugTextbox.Invoke(new MethodInvoker(delegate { debugTextbox.AppendText("Starting memory dump on " + xbox.XboxIP() + "...\n"); }));

            uint current = 0x80000000;
            uint target = 0x82FFFFFF;
            long paddingBytes = current;
            // Console.WriteLine(paddingBytes);
            string fileName = $"C:\\Projects\\C#\\XMem\\{filename}.bin";

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            FileStream stream = new FileStream(fileName, FileMode.Append, FileAccess.Write);

            for (int i = 0; i <  4; i++)
            {
                long div = paddingBytes / 4;
                byte[] paddingData = new byte[div];
                stream.Write(paddingData, 0, paddingData.Length);
            }

            stream.Close();

            while (current < target)
            {
                FileStream stream2 = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                byte[] memChunk = xbox.GetMemory(current, 0x00100000);
                current += 0x00100000;
                stream2.Write(memChunk, 0, memChunk.Length);
                stream2.Close();
            }

            debugTextbox.Invoke(new MethodInvoker(delegate { debugTextbox.AppendText("Memory dump completed\n"); }));

        }


    }
}
