using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace source_project
{
    public class HuffmanTree
    {
        HuffmanNode Root;
        public void encode(HuffmanNode root, string code, SortedDictionary<byte, string> codes)
        {
            if (root == null)
            {
                return;
            }
            if (root.IsLeaf())
            {
                codes[Convert.ToByte(root.letter)] = code;
            }
            encode(root.left, code + "0", codes);
            encode(root.right, code + "1", codes);
        }

        public HuffmanTree(HuffmanNode Root)
        {
            this.Root = Root;
        }

        public byte[] decode(byte[] ComFileBytes, int StartIndex, int ComFileLength, HuffmanNode Root)
        {
            int size = 0;
            HuffmanNode current = Root;
            List<byte> data = new List<byte>();
            for (int i = StartIndex; i < ComFileBytes.Length; ++i)
            {
                for (int bit = 1; bit <= 128; bit <<= 1)
                {
                    bool IsRankZero = (ComFileBytes[i] & bit) == 0;
                    if (IsRankZero)
                    {
                        current = current.left;
                    }
                    else
                    {
                        current = current.right;
                    }
                    if (current.left != null)
                    {
                        continue;
                    }
                    if (size++ < ComFileLength)
                    {
                        data.Add(Convert.ToByte(current.letter));
                    }
                    current = Root;
                }
            }
            return data.ToArray();
        }
    }
}
