namespace SadRobot.Functions.Media
{
    // Some Helper Struct to store Color-Data
    public struct ARGBColor8
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        /// <summary>
        /// Converts the given Pixel-Array into the BGRA-Format
        /// This will also work vice versa
        /// </summary>
        /// <param name="pixel"></param>
        public static void ConvertToBGRA(byte[] pixel)
        {
            byte tmp = 0;
            for (int i = 0; i < pixel.Length; i += 4)
            {
                tmp = pixel[i]; // store red
                pixel[i] = pixel[i + 2]; // Write blue into red
                pixel[i + 2] = tmp; // write stored red into blue
            }
        }
    }
}
