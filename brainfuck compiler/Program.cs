using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;

namespace brainfuck_compiler
{
    class Program
    {
        static int Compile(string inputFileName, string outputFileName)
        {
            using (StreamReader sr = File.OpenText(inputFileName))
            {
                string s;
                s = sr.ReadToEnd();
                using (StreamWriter fs = new StreamWriter(outputFileName))
                {
                    var jmp = 0u;
                    var stack = new Stack<uint>();
                    fs.WriteLine("extern exit");
                    fs.WriteLine("extern memset");
                    fs.WriteLine("extern _getch");
                    fs.WriteLine("extern putchar");
                    fs.WriteLine("extern malloc");
                    fs.WriteLine("section .text");
                    fs.WriteLine("global Start");
                    fs.WriteLine("Start:");
                    fs.WriteLine($"push dword {0x1000}");
                    fs.WriteLine("call malloc");
                    fs.WriteLine("add esp, 4");
                    fs.WriteLine($"push dword {0x1000}");
                    fs.WriteLine("push dword 0");
                    fs.WriteLine("push eax");
                    fs.WriteLine("call memset");
                    fs.WriteLine("mov ecx, eax");
                    fs.WriteLine($"mov ebx, {0x1000 / 8}");
                    fs.WriteLine("add esp, 12");
                    fs.WriteLine(";compiled code");
                    var plusTokens = 0;
                    var minusTokens = 0;
                    var leftTokens = 0;
                    var rightTokens = 0;
                    for (var i = 0; i < s.Length; i++)
                    {
                        switch (s[i])
                        {
                            case '<':
                            {
                                if (i + 1 < s.Length)
                                    if (s[i + 1] == '<')
                                    {
                                        leftTokens++;
                                    }
                                    else
                                    {
                                        fs.WriteLine($"sub ebx, {leftTokens + 1}");
                                            leftTokens = 0;
                                    }

                                break;
                                }
                            case '>':
                            {
                                if (i + 1 < s.Length)
                                    if (s[i + 1] == '>')
                                    {
                                        rightTokens++;
                                    }
                                    else
                                    {
                                        fs.WriteLine($"add ebx, {rightTokens + 1}");
                                        rightTokens = 0;
                                    }

                                break;
                                }
                            case '+':
                            {
                                if(i+1<s.Length)
                                    if (s[i + 1] == '+')
                                    {
                                        plusTokens++;
                                    }
                                else
                                    {
                                        fs.WriteLine($"add dword [ecx+ebx*4], {plusTokens+1}");
                                        plusTokens = 0;
                                    }
                                
                                break;
                            }
                            case '-':
                            {
                                if (i + 1 < s.Length)
                                    if (s[i + 1] == '-')
                                    {
                                        minusTokens++;
                                    }
                                    else
                                    {
                                        fs.WriteLine($"sub dword [ecx+ebx*4], {minusTokens + 1}");
                                            minusTokens = 0;
                                    }

                                break;
                                }
                            case '.':
                            {
                                fs.WriteLine("push ecx");
                                fs.WriteLine("push ebx");
                                fs.WriteLine("push dword [ecx+ebx*4]");
                                fs.WriteLine("call putchar");
                                fs.WriteLine("add esp, 4");
                                fs.WriteLine("pop ebx");
                                fs.WriteLine("pop ecx");
                                break;
                            }
                            case ',':
                            {
                                fs.WriteLine("push ecx");
                                fs.WriteLine("push ebx");
                                fs.WriteLine("call _getch");
                                fs.WriteLine("pop ebx");
                                fs.WriteLine("pop ecx");
                                fs.WriteLine("mov dword [ecx+ebx*4], eax");
                                break;
                            }
                            case '[':
                            {
                                stack.Push(jmp);
                                fs.WriteLine($"J{jmp}:");
                                jmp++;
                                break;
                            }
                            case ']':
                            {
                                var a = stack.Pop();
                                fs.WriteLine("cmp dword [ecx+ebx*4], 0");
                                fs.WriteLine($"jne J{a}");
                                break;
                            }
                                
                        }

                    }
                    fs.WriteLine(";end compiled code");
                    fs.WriteLine("xor eax, eax");
                    fs.WriteLine("push eax");
                    fs.WriteLine("call exit");
                    fs.WriteLine("add esp, 4");
                }
            }
            return 0;
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage bfc in.bf -o out.asm");
                return;
            }
            if (File.Exists(args[0]))
            {
                if (args[1] == "-o")
                    if (args.Length == 3)
                        Compile(args[0], args[2]);
                return;
            }
            else
            {
                Console.WriteLine("bfc: Input file not found");
                return;
            }

            
            
            
        }
    }
}
