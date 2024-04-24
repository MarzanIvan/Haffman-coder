using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.WebUI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace source_project
{
    public class FilePerformingTask
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Progress { get; set; }
        public int WorkInProgress { get; set; }
        public string Extension { get; set; }
        public byte[] FileBytes { get; set; }
        public int[] frequencies { get; set; }
        public string TaskState { get; set; }
        public Thread FilePerformingThread { get; set; }
        public string SuggestedFileType { get; set; }
        public string SuggestedFileChoose { get; set; }
        public SortedDictionary<byte, string> codes = new SortedDictionary<byte, string>();
        public string TaskIconPath { get; set; }
        public DateTime CreatingDate { get; set; }

        public Windows.Storage.StorageFile FileToPerform { get; set; }

        public async virtual void PerformFile() { }
        public virtual void RunPerforming() { }
        public virtual byte[] GetData() { return null; }

        public FilePerformingTask(Windows.Storage.StorageFile file, string TaskIconPath)
        {
            CreatingDate = DateTime.Now;
            this.TaskIconPath = TaskIconPath;
            SuggestedFileType = "";
            SuggestedFileChoose = "";
            FileToPerform = file;
            TaskState = "In process...";
            frequencies = new int[256];
            WorkInProgress = 3;
            Progress = 0;
            this.Path = file.Path;
            this.Name = file.DisplayName;

        }
    }


    public class FileDecompressionTask : FilePerformingTask
    {
        public byte[] DecompressedData { get; set; }

        public FileDecompressionTask(StorageFile file, string TaskIconPath) : base(file, TaskIconPath)
        {
            SuggestedFileChoose = "Plain Text";
            SuggestedFileType = ".txt";
        }

        public override byte[] GetData()
        {
            return DecompressedData;
        }

        public override void RunPerforming()
        {
            FilePerformingThread = new Thread(() => PerformFile());
            FilePerformingThread.Start();
            Thread WaitingThread = new Thread(() => MonitorTaskState());
            WaitingThread.Start();
        }

        public async void MonitorTaskState()
        {
            FilePerformingThread.Join();
            Progress = 3;
            TaskState = "Decompression is done. Click me)";
        }

        public void GetHeader(byte[] ComFileBytes, out int Length)
        {
            Length = ComFileBytes[0] | (ComFileBytes[1] << 8) | (ComFileBytes[2] << 16) | (ComFileBytes[3] << 24);
            frequencies = new int[256];
            for (int i = 0; i < 256; ++i)
            {
                frequencies[i] = ComFileBytes[i + 4];
            }
            return;
        }

        public async override void PerformFile()
        {
            var fileProperties = await FileToPerform.GetBasicPropertiesAsync();

            byte[] ComFileBytes = new byte[fileProperties.Size];
            int j = 0;

            using (var inputStream = await FileToPerform.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            using (var stream = streamReader.BaseStream)
            {
                stream.Position = 0;
                for (ulong i = 0; i < fileProperties.Size; ++i)
                {
                    ComFileBytes[j++] = (byte)stream.ReadByte();
                }
            }
            int ComFileLength = 0;
            int StartIndex = 260;
            GetHeader(ComFileBytes, out ComFileLength);
            PriorityQueue<HuffmanNode> NodesQueue = new PriorityQueue<HuffmanNode>();
            for (int i = 0; i < 256; ++i)
            {
                if (frequencies[i] > 0)
                {
                    NodesQueue.Push(frequencies[i], new HuffmanNode(Convert.ToByte(i), frequencies[i], null, null));
                }
            }
            while (NodesQueue.Size() > 1)
            {
                HuffmanNode FirstNode = new HuffmanNode();
                FirstNode = NodesQueue.Pop();
                HuffmanNode SecondNode = new HuffmanNode();
                SecondNode = NodesQueue.Pop();
                int SumOfFrequencies = FirstNode.frequency + SecondNode.frequency;

                NodesQueue.Push(SumOfFrequencies, new HuffmanNode(0, SumOfFrequencies, FirstNode, SecondNode));
            }
            HuffmanNode Root = new HuffmanNode();
            Root = NodesQueue.Pop();
            HuffmanTree Tree = new HuffmanTree(Root);
            DecompressedData = Tree.decode(ComFileBytes, StartIndex, ComFileLength, Root);
            return;
        }
    }
    public class FileCompressionTask : FilePerformingTask
    {

        public byte[] header { get; set; }
        public byte[] body { get; set; }

        public override byte[] GetData()
        {
            return header.Concat(body).ToArray();
        }

        public byte[] CreateHeader(int AmountLetters, int[] frequencies)
        {
            List<byte> header = new List<byte>(256 + 4);
            header.Add(Convert.ToByte(AmountLetters & 255));
            header.Add(Convert.ToByte((AmountLetters >> 8) & 255));
            header.Add(Convert.ToByte((AmountLetters >> 16) & 255));
            header.Add(Convert.ToByte((AmountLetters >> 24) & 255));
            for (int i = 0; i < 256; ++i)
            {
                header.Add(Convert.ToByte(frequencies[i]));
            }
            return header.ToArray();
        }

        public byte[] CreateBits()
        {
            List<byte> bits = new List<byte>(FileBytes.Length);
            byte sum = 0;
            byte bit = 1;
            for (int i = 0; i < FileBytes.Length; ++i)
                foreach (var code in codes[FileBytes[i]])
                {
                    if (code == '1')
                    {
                        sum |= bit;
                    }
                    if (bit < 128)
                    {
                        bit <<= 1;
                    }
                    else
                    {
                        bits.Add(sum);
                        sum = 0;
                        bit = 1;
                    }
                }
            if (bit > 1)
            {
                bits.Add(sum);
            }

            return bits.ToArray();
        }

        public override void RunPerforming()
        {
            FilePerformingThread = new Thread(() => compress());
            FilePerformingThread.Start();
            Thread WaitingThread = new Thread(() => MonitorTaskState());
            WaitingThread.Start();
        }

        public void MonitorTaskState()
        {
            FilePerformingThread.Join();
            Progress = 3;
            TaskState = "Compression is done. Click me)";
        }
        public async void compress()
        {

            var fileProperties = await FileToPerform.GetBasicPropertiesAsync();
            FileBytes = new byte[fileProperties.Size];
            ++Progress;
            int j = 0;

            using (var inputStream = await FileToPerform.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            using (var stream = streamReader.BaseStream)
            {
                stream.Position = 0;
                for (ulong i = 0; i < fileProperties.Size; ++i)
                {
                    var newbyte = (byte)stream.ReadByte();
                    FileBytes[j++] = newbyte;
                    ++frequencies[newbyte];
                }
            }

            int MaxFrequency = frequencies.Max();
            if (MaxFrequency > 255)
            {
                for (int i = 0; i < 256; ++i)
                {
                    if (frequencies[i] > 0)
                    {
                        frequencies[i] = 1 + frequencies[i] * 255 / (MaxFrequency + 1);
                    }
                }
            }

            PriorityQueue<HuffmanNode> NodesQueue = new PriorityQueue<HuffmanNode>();
            for (int i = 0; i < 256; ++i)
            {
                if (frequencies[i] > 0)
                {
                    NodesQueue.Push(frequencies[i], new HuffmanNode(Convert.ToByte(i), frequencies[i], null, null));
                }
            }

            while (NodesQueue.Size() > 1)
            {
                HuffmanNode FirstNode = new HuffmanNode();
                FirstNode = NodesQueue.Pop();
                HuffmanNode SecondNode = new HuffmanNode();
                SecondNode = NodesQueue.Pop();
                int SumOfFrequencies = FirstNode.frequency + SecondNode.frequency;

                NodesQueue.Push(SumOfFrequencies, new HuffmanNode(0, SumOfFrequencies, FirstNode, SecondNode));
            }
            HuffmanNode Root = new HuffmanNode();
            Root = NodesQueue.Pop();
            HuffmanTree Tree = new HuffmanTree(Root);
            string code = "";
            Tree.encode(Root, code, codes);
            
            header = CreateHeader((int)fileProperties.Size, frequencies);
            body = CreateBits();

            var NewNotification = new ToastContentBuilder();
            NewNotification.AddText(FileToPerform.DisplayName + "\nГотов к сжатию:)");
            NewNotification.Show();

        }

        public FileCompressionTask(StorageFile file, string TaskIconPath) : base(file, TaskIconPath)
        {
            SuggestedFileChoose = "huff";
            SuggestedFileType = ".huff";
        }
    }
}
