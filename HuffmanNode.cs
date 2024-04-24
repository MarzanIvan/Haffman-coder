using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace source_project
{
    public class HuffmanNode: IComparable<HuffmanNode>
    {
        public byte letter { get; set; }

        public int frequency { get; set; }

        public HuffmanNode left { get; set; }

        public HuffmanNode right { get; set; }
        
        public bool IsLeaf()
        {
            if (left == null && right == null)
            {
                return true;
            } return false;
        }

        public int CompareTo(HuffmanNode other)
        {
            if (this.frequency < other.frequency)
            {
                return -1;
            } else if (this.frequency > other.frequency)
            {
                return 1;
            } else
            {
                return 0;
            }
        }
        public HuffmanNode()
        {
            this.letter = 0;
            this.frequency = 0;
            this.left = null;
            this.right = null;
        }
        public HuffmanNode(byte letter, int frequency, HuffmanNode left, HuffmanNode right)
        {
            this.letter = letter;
            this.frequency = frequency;
            this.left = left;
            this.right = right;
        }
    }
}
