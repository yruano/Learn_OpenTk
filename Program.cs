﻿namespace BasicOpenTk
{
    class Program
    {
        static void Main(string[] args)
        {
            // using (Triangle triangle = new Triangle())
            // {
            //     triangle.Run();
            // }
            
            // using (Square square = new Square())
            // {
            //     square.Run();
            // }

            // using (Boxes boxes = new Boxes())
            // {
            //     boxes.Run();
            // }

            // using (Star star = new Star()) 
            // {
            //     star.Run();
            // }
            Model model = new("Model", 1280, 800);
            model.Run();
        }
    }
}