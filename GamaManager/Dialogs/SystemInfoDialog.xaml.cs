using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Management;
using System.Diagnostics;

namespace GamaManager.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SystemInfoDialog.xaml
    /// </summary>
    public partial class SystemInfoDialog : Window
    {
        public SystemInfoDialog()
        {
            InitializeComponent();

            Initialize();
        
        }

        public void Initialize ()
        {
            var driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementObject d in driveQuery.Get())
            {
                var partitionQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", d.Path.RelativePath);
                var partitionQuery = new ManagementObjectSearcher(partitionQueryText);
                foreach (ManagementObject p in partitionQuery.Get())
                {
                    var logicalDriveQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", p.Path.RelativePath);
                    var logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                    foreach (ManagementObject ld in logicalDriveQuery.Get())
                    {
                        ulong totalSpace = Convert.ToUInt64(ld.Properties["Size"].Value);
                        ulong totalSpaceInGb = totalSpace / 1024 / 1024 / 1024;
                        string rawTotalSpaceInGb = totalSpaceInGb.ToString();
                        driveSpaceSizeLabel.Text = "Размеры SSD: " + rawTotalSpaceInGb + " Гб";
                    }
                }
            }

            ManagementObjectSearcher search = new ManagementObjectSearcher("Select * From Win32_PhysicalMemory");
            foreach (ManagementObject ram in search.Get())
            {
                ulong size = Convert.ToUInt64(ram.GetPropertyValue("Capacity"));
                ulong totalSizeInGb = size / 1024 / 1024 / 1024;
                string rawTotalSizeInGb = totalSizeInGb.ToString();
                ramSizeLabel.Text = "Оперативная память: " + rawTotalSizeInGb + " Гб";
            }

            search = new ManagementObjectSearcher("Select * From Win32_SoundDevice");
            foreach (ManagementObject ram in search.Get())
            {
                string soundCardCaption = Convert.ToString(ram.GetPropertyValue("Caption"));
                audioDeviceLabel.Text = "Аудиоустройство: " + soundCardCaption;
            }

            search = new ManagementObjectSearcher("Select * From Win32_PhysicalMemory");
            foreach (ManagementObject ram in search.Get())
            {
                ulong size = Convert.ToUInt64(ram.GetPropertyValue("Capacity"));
                ulong totalSizeInGb = size / 1024 / 1024 / 1024;
                string rawTotalSizeInGb = totalSizeInGb.ToString();
                ramSizeLabel.Text = "Оперативная память: " + rawTotalSizeInGb + " Гб";
            }

            search = new ManagementObjectSearcher("Select * From Win32_SoundDevice");
            foreach (ManagementObject ram in search.Get())
            {
                string soundCardCaption = Convert.ToString(ram.GetPropertyValue("Caption"));
                audioDeviceLabel.Text = "ОС: " + soundCardCaption;
            }

            search = new ManagementObjectSearcher("Select * From Win32_OperatingSystem");
            foreach (ManagementObject ram in search.Get())
            {
                string osVersion = Convert.ToString(ram.GetPropertyValue("Version"));
                osVersionLabel.Text = "ОС: " + osVersion;
            }

            search = new ManagementObjectSearcher("Select * From Win32_Volume");
            foreach (ManagementObject ram in search.Get())
            {
                string fs = Convert.ToString(ram.GetPropertyValue("FileSystem"));
                osFsLabel.Text = fs + " поддерживается";
            }

            computerTypeLabel.Text = "Тип: ";
            search = new ManagementObjectSearcher("Select * From Win32_SystemEnclosure");
            foreach (ManagementObject ram in search.Get())
            {
                UInt16[] types = ((UInt16[])(ram.GetPropertyValue("ChassisTypes")));
                foreach (UInt16 type in types)
                {
                    string rawType = type.ToString();
                    bool isPC = type == 3;
                    bool isLaptop = type == 9;
                    bool isNoteBook = type == 10;
                    bool isLaptopOrNoteBook = isLaptop || isNoteBook;
                    if (isPC)
                    {
                        rawType = "ПК";
                    }
                    else if (isLaptopOrNoteBook)
                    {
                        rawType = "Ноутбук";
                    }
                    computerTypeLabel.Text += rawType;
                }
            }

            search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");
            foreach (ManagementObject ram in search.Get())
            {
                string producer = Convert.ToString(ram.GetPropertyValue("Manufacturer"));
                computerProducerLabel.Text = "Производитель: " + producer;
                string model = Convert.ToString(ram.GetPropertyValue("Model"));
                computerModelLabel.Text = "Модель: " + model;
            }

            search = new ManagementObjectSearcher("Select * From Win32_PointingDevice");
            foreach (ManagementObject ram in search.Get())
            {
                string status = Convert.ToString(ram.GetPropertyValue("Status"));
                bool isOKStatus = status == "OK";
                string statusMsg = "не поддерживается";
                if (isOKStatus)
                {
                    statusMsg = "поддерживается";
                }
                computerSensorInputLabel.Text = "Сенсорный ввод " + statusMsg;
            }

            search = new ManagementObjectSearcher("Select * From Win32_Processor");
            foreach (ManagementObject ram in search.Get())
            {
                string producer = Convert.ToString(ram.GetPropertyValue("Manufacturer"));
                processorProducerLabel.Text = "Производитель процессора: " + producer;
                UInt16 family = Convert.ToUInt16(ram.GetPropertyValue("Family"));
                processorFamilyLabel.Text = "Семейство процессора: " + family;
                string model = Convert.ToString(ram.GetPropertyValue("Name"));
                processorModelLabel.Text = "Модель процессора: " + model;
                string stepping = Convert.ToString(ram.GetPropertyValue("Stepping"));
                processorSteppingLabel.Text = "Степпинг процессора: " + stepping;
                uint frequency = Convert.ToUInt32(ram.GetPropertyValue("CurrentClockSpeed"));
                string rawFrequency = frequency.ToString();
                processorFrequencyLabel.Text = "Тактовая частота: " + rawFrequency;
                uint cores = Convert.ToUInt32(ram.GetPropertyValue("NumberOfLogicalProcessors"));
                string rawCores = cores.ToString();
                processorLogicalCoresLabel.Text = "Логических процессоров: " + rawCores;
                cores = Convert.ToUInt32(ram.GetPropertyValue("NumberOfCores"));
                rawCores = cores.ToString();
                processorPhysicalCoresLabel.Text = "Физических процессоров: " + rawCores;
            }

            int videoCardsCount = 0;
            search = new ManagementObjectSearcher("Select * From Win32_VideoController");
            foreach (ManagementObject gpu in search.Get())
            {
                videoCardsCount++;
                string model = Convert.ToString(gpu.GetPropertyValue("Name"));
                videoCardModelLabel.Text = "Модель: " + model;
                UInt16 bus = Convert.ToUInt16(gpu.GetPropertyValue("ProtocolSupported"));
                string rawBus = bus.ToString();
                videoCardBusLabel.Text = "Осн. тип шины: " + rawBus;
                uint ram = Convert.ToUInt32(gpu.GetPropertyValue("AdapterRAM"));
                uint ramInMb = ram / 1024 / 1024;
                string rawRamInMb = ramInMb.ToString();
                videoCardRamLabel.Text = "Осн. видеопамять: " + rawRamInMb;
                string rawVideoCardsCount = videoCardsCount.ToString();
                videoCardCountLabel.Text = "Количество логических видеокарт: " + rawVideoCardsCount;
                string version = Convert.ToString(gpu.GetPropertyValue("SpecificationVersion"));
                videoCardVersionLabel.Text = "Версия: " + version;
                string id = Convert.ToString(gpu.GetPropertyValue("DeviceID"));
                videoCardProducerIdLabel.Text = "ID производителя: " + id;
                videoCardDeviceIdLabel.Text = "ID карты: " + id;
                uint rate = Convert.ToUInt32(gpu.GetPropertyValue("CurrentRefreshRate"));
                string rawRate = id.ToString();
                videoCardRateLabel.Text = "Частота обновления: " + rawRate;
                string driverVersion = Convert.ToString(gpu.GetPropertyValue("DriverVersion"));
                videoCardOpenGLDriverVersionLabel.Text = "Версия OpenGL: " + driverVersion;
                videoCardDirectXDriverVersionLabel.Text = "Версия драйвера DirectX: " + driverVersion;
                videoCardDriverVersionLabel.Text = "Версия драйвера: " + driverVersion;
                videoCardDirectXDriverLabel.Text = "Драйвер DirectX: " + driverVersion;
                uint depth = Convert.ToUInt32(gpu.GetPropertyValue("ColorTableEntries"));
                string rawDepth = depth.ToString();
                videoCardDepthLabel.Text = "Глубина цвета: " + rawDepth;
                string date = Convert.ToString(gpu.GetPropertyValue("DriverDate"));
                //string rawDate = date.ToLongDateString();
                videoCardDateLabel.Text = "Дата выхода: " + date;
            }

            search = new ManagementObjectSearcher("Select * From WmiMonitorBasicDisplayParams");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "Select * From WmiMonitorBasicDisplayParams");
            int displaysCount = 0;
            foreach (ManagementObject gpu in searcher.Get())
            {
                displaysCount++;
                double width = Convert.ToDouble(gpu.GetPropertyValue("MaxHorizontalImageSize")) / 2.54;
                double height = Convert.ToDouble(gpu.GetPropertyValue("MaxVerticalImageSize")) / 2.54;
                double diagonal = Math.Pow((Math.Sqrt(height) + Math.Sqrt(width)), 2);
                int parsedDiagonal = ((int)(diagonal));
                string rawScreenSize = parsedDiagonal.ToString();
                videoCardScreenSizeLabel.Text = "Размер осн. экрана: " + rawScreenSize;
                string rawDisplaysCount = displaysCount.ToString();
                videoCardDisplaysCountLabel.Text = "Кол-во экранов: " + rawDisplaysCount;
            }

            search = new ManagementObjectSearcher("Select * From Win32_DesktopMonitor");
            foreach (ManagementObject gpu in search.Get())
            {
                uint width = Convert.ToUInt32(gpu.GetPropertyValue("ScreenWidth"));
                uint height = Convert.ToUInt32(gpu.GetPropertyValue("ScreenHeight"));
                string rawResolutionWidth = width.ToString();
                string rawResolutionHeight = height.ToString();
                string rawResolution = rawResolutionWidth + "X" + rawResolutionHeight;
                videoCardScreenResolutionLabel.Text = "Разрешение осн. экрана: " + rawResolution;
                videoCardDesktopResolutionLabel.Text = "Разрешение рабочего стола: " + rawResolution;
            }

        }

    }
}
