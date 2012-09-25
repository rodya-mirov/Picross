using System;

namespace Picross
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PicrossGame game = new PicrossGame())
            {
                game.Run();
            }
        }
    }
#endif
}

