// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

using var converter = new LoopToIosConverter.Common.LoopToIosConverter("C:\\Users\\tomer\\Downloads\\Loop Habits Backup 2022-03-26 165755.db");
await converter.ConvertAsync(@"C:\Users\tomer\Downloads\converted.csv");