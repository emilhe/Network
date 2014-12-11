using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public static class ColorController
    {

        private static readonly Dictionary<int, Color> Colors = new Dictionary<int, Color>
        {
                        {0,Color.Red},
                        {1,Color.Blue},
                        {2,Color.Green},
                        {3,Color.Gold},
                        {4,Color.Purple},
                        {5,Color.Orange},
                        {6,Color.Black}
        };

        private static int _mIdx;

        public static Color NextColor()
        {
            return Colors[_mIdx++];
        }

    }
}
