using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace multiClient
{
   
    public partial class Form1 : Form
    {
        bool sendIndex;
        byte[] bufferKey,bufferIV;
        Socket sck;
        EndPoint epLocal, epRemote;
        public Form1()
        {
            InitializeComponent();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);

            textLocalIp.Text = GetLocalIp();
            textFriendsIp.Text = GetLocalIp();
        }
        private string GetLocalIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        private void MessageCallBack(IAsyncResult aResult)
        {
            ////int index = 0;
            ////if (sendIndex == true)
            ////{
            ////    try
            ////    {
            ////        int size = sck.EndReceiveFrom(aResult, ref epRemote);
            ////        if (size > 0)
            ////        {
            ////            byte[] receivedIndex = (byte[])aResult.AsyncState;
            ////            index = Convert.ToInt32(receivedIndex);
            ////            comboBox1.SelectedIndex = index;
            ////        }
            ////        byte[] buffer = new byte[500];
            ////        sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            ////        sendIndex = false;
            ////    }
            ////    catch (Exception e)
            ////    {
            ////        sendIndex = false;
            ////        MessageBox.Show(e.ToString());
            ////    }
            ////}
            //if (comboBox1.SelectedIndex == 0)
            //{
                try
                {
                    int size = sck.EndReceiveFrom(aResult, ref epRemote);
                    if (size > 0)
                    {
                        //MessageBox.Show("size: " + size.ToString());
                        using (TripleDESCryptoServiceProvider myTripleDES = new TripleDESCryptoServiceProvider())
                        {

                            //Encrypt the string to an array of bytes.
                            byte[] receivedData = (byte[])aResult.AsyncState;
                            string roundtrip = "";
                            Message m = (Message)BinaryDeserialize(receivedData);
                            comboBox1.SelectedIndex = m.selection;

                            if (comboBox1.SelectedIndex == 0)
                            {
                                // Decrypt the bytes to a string.
                                roundtrip = DecryptStringFromBytes(m.msg, m.Key, m.IV);
                            }
                            else
                            {
                                roundtrip = DecryptStringFromBytes_Aes(m.msg, m.Key, m.IV);
                            }
                            
                           

                            ASCIIEncoding encoding = new ASCIIEncoding();
                            listMessage.Items.Add("Friend: " + roundtrip);
                            listMessage.SelectedIndex = listMessage.Items.Count - 1;
                        }


                    }
                    byte[] buffer = new byte[500];
                    sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.ToString());
                }
            //}
            //else
            //{
            //    try
            //    {

            //        int size = sck.EndReceiveFrom(aResult, ref epRemote);
            //        if (size > 0)
            //        {
            //            //MessageBox.Show("size: " + size.ToString());
            //            using (Aes myAes = Aes.Create())
            //            {

            //                //Encrypt the string to an array of bytes.
            //                byte[] receivedData = (byte[])aResult.AsyncState;

            //                Message m = (Message)BinaryDeserialize(receivedData);

                            
            //                // Decrypt the bytes to a string.
            //                string roundtrip = DecryptStringFromBytes_Aes(m.msg, m.Key, m.IV);

            //                ASCIIEncoding encoding = new ASCIIEncoding();
            //                listMessage.Items.Add("Friend: " + roundtrip);
            //                listMessage.SelectedIndex = listMessage.Items.Count - 1;
            //            }


            //        }
            //        byte[] buffer = new byte[500];
            //        sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Error: {0}", ex.Message);
            //    }
            //}
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);

                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
                sck.Connect(epRemote);

                byte[] buffer = new byte[500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None ,ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                button1.BackgroundImage = null; 
                button1.BackgroundImage = Image.FromFile("images\\stop.png");
                //button1.Text = "Connected";
                button1.Enabled = false;
                button2.Enabled = true;
                textMessage.Focus();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
            comboBox1.SelectedIndex = 0;
        }
       
        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                try
                {
                    using (TripleDESCryptoServiceProvider myTripleDES = new TripleDESCryptoServiceProvider())
                    {
                        //Encrypt the string to an array of bytes.
                        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                        bufferKey = myTripleDES.Key;
                        bufferIV = myTripleDES.IV;
                        byte[] encrypted = EncryptStringToBytes(textMessage.Text, bufferKey, bufferIV);

                        Message p = new Message();
                        p.msg = encrypted;
                        p.Key = bufferKey;
                        p.IV = bufferIV;
                        p.selection = comboBox1.SelectedIndex;
                        label5.Text = Encoding.ASCII.GetString(p.msg);
                        label6.Text = Encoding.ASCII.GetString(p.Key);
                        byte[] sending = BinarySerialize(p);


                        sck.Send(sending);
                        listMessage.Items.Add("You: " + textMessage.Text);
                        textMessage.Clear();
                    }
                }


                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                try
                {
                    // Create a new instance of the Aes
                    // class.  This generates a new key and initialization 
                    // vector (IV).
                    using (Aes myAes = Aes.Create())
                    {
                        bufferKey = myAes.Key;
                        bufferIV = myAes.IV;
                        // Encrypt the string to an array of bytes.
                        byte[] encrypted = EncryptStringToBytes_Aes(textMessage.Text, bufferKey, bufferIV);
                        Message p = new Message();
                        p.msg = encrypted;
                        p.Key = bufferKey;
                        p.IV = bufferIV;
                        p.selection = comboBox1.SelectedIndex;
                        label5.Text = Encoding.ASCII.GetString(p.msg);
                        label6.Text = Encoding.ASCII.GetString(p.Key);
                        byte[] sending = BinarySerialize(p);


                        sck.Send(sending);
                        listMessage.Items.Add("You: " + textMessage.Text);
                        textMessage.Clear();

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            //int intValue = comboBox1.SelectedIndex;

            //byte[] intBytes = BitConverter.GetBytes(intValue);
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(intBytes);
            //byte[] result = intBytes;
            //sck.Send(intBytes);
            //sendIndex = true;


        }
        public static byte[] BinarySerialize(object graph) //Serialize
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, graph);

                return stream.ToArray();
            }
        }

        public static object BinaryDeserialize(byte[] buffer)//Deserialize
        {
            using (var stream = new MemoryStream(buffer))
            {
                var formatter = new BinaryFormatter();

                return formatter.Deserialize(stream);
            }
        }

       

        //DES ALGORITHM METHODS
        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an TripleDESCryptoServiceProvider object
            // with the specified key and IV.
            using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
            {
                tdsAlg.Key = Key;
                tdsAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = tdsAlg.CreateEncryptor(tdsAlg.Key, tdsAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }
        //DES ALGORITHM METHODS
        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an TripleDESCryptoServiceProvider object
            // with the specified key and IV.
            using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
            {
                tdsAlg.Key = Key;
                tdsAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = tdsAlg.CreateDecryptor(tdsAlg.Key, tdsAlg.IV);

                // Create the streams used for decryption.

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            return plaintext;
        }

        //AES ALOGRITHM METHODS
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }
        //AES ALGORITHM METHODS
        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting 

                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }
    [Serializable]
    public class Message
    {

        public byte[] msg { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
        public int selection { get; set; }
    }

}
