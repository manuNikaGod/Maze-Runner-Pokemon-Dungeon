using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;


namespace MazeRunners
{
  
    public class Program
    {
        public static void Main()
        {
            Game game = new Game();
            
            game.ShowMenu();

            Console.Read();
        }
    }


}


