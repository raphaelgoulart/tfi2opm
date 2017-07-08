using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TFI2OPM {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("TFI2OPM by raphaelgoulart.");
                Console.WriteLine("Usage: Drag and drop one or multiple .tfi files into this executable file, and the program will write an 'output.opm' file.");
                Console.WriteLine("You can drag up to 128 .tfi files, since that's the maximum amount of patches VOPM supports.");
                Console.WriteLine("Press any key to exit...");
            } else if (args.Length > 128) {
                Console.WriteLine("You cannot convert more than 128 patches into a single .opm files! Try draging less files into the converter.");
                Console.WriteLine("Press any key to exit...");
            } else {
                Console.WriteLine("Starting program...");
                Array.Sort(args, new OrdinalStringComparer());
                List<Instrument> ins = new List<Instrument>();
                int count = 0;
                foreach (var path in args) {
                    Console.WriteLine("Reading file: " + ReadName(path) + "...");
                    try {
                        ins.Add(ReadInstrument(path, count));
                        Console.WriteLine("Finished reading file.");
                        count++;
                    } catch (Exception e) {
                        Console.WriteLine("Error reading file: " + e);
                    }
                }
                Console.WriteLine("Writing output.opm file...");
                try {
                    WriteInstrument(ins);
                    Console.WriteLine("Finished writing file.");
                } catch (Exception e) {
                    Console.WriteLine("Error writing file: " + e);
                }
                Console.WriteLine("Finished! Press any key to exit...");
            }
            Console.ReadKey();
        }

        static Instrument ReadInstrument(string path, int num) {
            var data = File.ReadAllBytes(path);
            if (data.Length != 42) throw new Exception("Invalid .tfi file");
            else {
                Instrument ins = new Instrument();
                ins.num = num;
                ins.name = ReadName(path);
                ins.al = data[0];
                ins.fb = data[1];
                ins.m1 = ReadOperator(data, 0);
                ins.m2 = ReadOperator(data, 1);
                ins.c1 = ReadOperator(data, 2);
                ins.c2 = ReadOperator(data, 3);
                return ins;
            }
        }

        static string ReadName(string path) {
            var arr = path.Split('\\');
            var name = arr[arr.Length - 1];
            return name.Substring(0, name.Length - 4);
        }

        static Operator ReadOperator(byte[] data, int op) {
            var offset = 10 * op + 2;
            Operator o = new Operator();
            o.mt = data[0 + offset];
            o.dt = data[1 + offset];
            o.tl = data[2 + offset];
            o.rs = data[3 + offset];
            o.ar = data[4 + offset];
            o.dr = data[5 + offset];
            o.sr = data[6 + offset];
            o.rr = data[7 + offset];
            o.sl = data[8 + offset];
            o.eg = data[9 + offset];
            return o;
        }

        static void WriteInstrument(List<Instrument> list) {
            using (var output = new StreamWriter("output.opm")) {
                output.WriteLine("//TFI2OPM by raphaelgoulart");
                output.WriteLine();
                foreach (var ins in list) {
                    output.WriteLine("@:" + ins.num + " " + ins.name);
                    output.WriteLine("LFO: 0 0 0 0 0");
                    output.WriteLine("CH: 64 " + ins.fb + " " + ins.al + " 0 0 120 0");
                    output.WriteLine(WriteOperator("M1", ins.m1));
                    output.WriteLine(WriteOperator("C1", ins.c1));
                    output.WriteLine(WriteOperator("M2", ins.m2));
                    output.WriteLine(WriteOperator("C2", ins.c2));
                    output.WriteLine();
                }
            }
        }

        static string WriteOperator(string name, Operator op) {
            var str = name + ": ";
            str += op.ar + " ";
            str += op.dr + " ";
            str += op.sr + " ";
            str += op.rr + " ";
            str += op.sl + " ";
            str += op.tl + " ";
            str += op.rs + " ";
            str += op.mt + " ";
            str += op.dt + " 0 ";
            str += op.eg;
            return str;
        }
    }

    class Instrument {
        public int num = 0;
        public string name;
        public byte al = 0;
        public byte fb = 0;
        public Operator m1;
        public Operator c1;
        public Operator m2;
        public Operator c2;
    }

    class Operator {
        public byte mt = 0;
        public byte dt = 0;
        public byte tl = 0;
        public byte rs = 0;
        public byte ar = 0;
        public byte dr = 0;
        public byte sr = 0;
        public byte rr = 0;
        public byte sl = 0;
        public byte eg = 0;
    }

    class OrdinalStringComparer:IComparer<string> {
        public int Compare(string x, string y) {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var splitX = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
            var splitY = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
            var comparer = 0;
            for (var i = 0; comparer == 0 && i < splitX.Length; i++) {
                if (splitY.Length <= i) comparer = 1; // x > y
                int numericX = -1, numericY = -1;
                if (int.TryParse(splitX[i], out numericX)) 
                    if (int.TryParse(splitY[i], out numericY)) comparer = numericX - numericY;
                    else comparer = 1; // x > y
                else comparer = string.Compare(splitX[i], splitY[i], StringComparison.CurrentCultureIgnoreCase);
            }
            return comparer;
        }
    }
}
