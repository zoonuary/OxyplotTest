using System.Windows.Media;

namespace OxyTest.Services
{
    public class RandomColors
    {
        public RandomColors()
        {
            Index = 0;
        }

        private int Index; 
        public Color GetNextColor()
        {
            return Palette[Index++ % Palette.Length];
        }

        private readonly Color[] Palette = new Color[]
        {
            Color.FromRgb(0,0,0), //black
            Color.FromRgb(255,0,0),//R
            Color.FromRgb(0,128,0), //G
            Color.FromRgb(0,0,255) //B
        };
    }
}
