using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace PassAuth
{


    public partial class Form1 : Form
    {
        public string str;

        public Form1()
        {
            InitializeComponent();
            search_external_drives(comboBox1);
            comboBox1.SelectedIndex = 0;
        }

        private int i = 3;

        private void search_external_drives(ComboBox input) //поиск внешних носителей
        {
            string mydrive;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady && (d.DriveType == DriveType.Removable))
                {
                    mydrive = d.Name;
                    input.Items.Add(mydrive);
                }
            }
            if (input.Items.Count == 0)
            {
                input.Items.Add("Внешние носители отсутствуют");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*if (Directory.Exists(str))
            {
                
                string pas = textBox1.Text;
                string pass;
                FileInfo fileInf = new FileInfo(str + "pass.txt");
                if (fileInf.Exists)
                {
                    FileStream file = new FileStream(str + "pass.txt", FileMode.Open);
                    StreamReader reader = new StreamReader(file);
                    pass = reader.ReadLine();
                    reader.Close();
                    key Key = new key();
                    Key.Val = textBox2.Text;
                    Key.Dir = str;
                    pas = Encode(pas, Key.Val);
                    if (pas == pass)
                    {
                        Form ifrm = new Form2(Key);
                        ifrm.Show();
                        this.Hide();
                    }
                    else
                    {
                        i--;
                        MessageBox.Show("Wrong password or key! You have " + i + " chances!");
                    }
                }
                else
                {
                    FileStream file = new FileStream(str + "pass.txt", FileMode.Create);
                    StreamWriter writer = new StreamWriter(file);
                    Random rnd = new Random();
                    int i = 1 + rnd.Next(100);
                    pas = Encode(pas, GenerateKeyWord(pas.Length, i));
                    writer.WriteLine(pas);
                    MessageBox.Show("Your key: " + GenerateKeyWord(pas.Length, i));
                    writer.Close();
                    key Key = new key();
                    Key.Val = GenerateKeyWord(pas.Length, i);
                    Key.Dir = str;
                    Form ifrm = new Form2(Key);
                    ifrm.Show();
                    this.Hide();
                }
                if (i == 0)
                {
                    this.Close();
                }
            }

            else
            {
                MessageBox.Show("Dir doesn't exist!");
            }
            */


            string password_1 = "";
            password_1 = textBox1.Text;
            string drive = "\\\\.\\" + comboBox1.Text.Remove(2);
            string pas = textBox1.Text;
            key Key = new key();
            Key.Dir = comboBox1.Text.Remove(2); ;
            byte[] ByteBuffer = new byte[512];//задаем размер буфера
            byte[] temp = new byte[8];
            bool flag = true;
            FileReader fr = new FileReader();
            if (fr.OpenRead(drive)) //вызов для чтения
            {
                int count = fr.Read(ByteBuffer, 512);

                for (int i = 54; i < 62; i++)
                {
                    temp[i - 54] = ByteBuffer[i];
                }
                fr.Close();

                if (temp[0] == 0)//пароль еще не записан на флешке
                {
                    fr.OpenWrite(drive);
                    {
                        Random rnd = new Random();
                        int l = 1 + rnd.Next(100);
                        Key.Val = GenerateKeyWord(pas.Length, l);
                        MessageBox.Show("Your key: " + Key.Val);
                        string codpass = Encode(pas, Key.Val);
                        byte[] oldpass = new byte[8];
                        for (int i = 0; i < codpass.Length; i++)
                        {
                            oldpass[i] = (byte)codpass[i];
                        }
                        for (int i = 54; i < 62; i++)
                        {
                            ByteBuffer[i] = oldpass[i - 54];
                        }
                        count = fr.Write(ByteBuffer, 0, 512);
                        this.Hide();
                        Form2 f = new Form2(Key); //Вывод на экран окна успешного ввода
                        f.Show();
                    }
                    fr.Close();
                }
                else
                {
                    Key.Val = textBox2.Text;
                    string codpass = Encode(pas, Key.Val);
                    byte[] oldpass = new byte[8];
                    for (int i = 0; i < codpass.Length; i++)
                    {
                        oldpass[i] = (byte)codpass[i];
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        if (temp[i] != oldpass[i])
                            flag = false;
                    }
                    if (flag != false)
                    {
                        //codpass = null;
                        //System.Diagnostics.Process.Start(@"h:\\");
                        this.Hide();
                        Form2 f = new Form2(Key); //Вывод на экран окна успешного ввода
                        f.Show();
                    }
                    else
                    {
                        MessageBox.Show("Попыток:" + i);
                        i--;
                        if (i < 1)
                            Application.Exit();
                        textBox1.Clear();
                    }
                }
            }
            
        }

        public static char[] characters = new char[] {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
           'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','X','X','0','1','2','3','4','5','6','7','8','9','_'};

        public int N = characters.Length;

        public string Encode(string input, string keyword)
        {
            input = input.ToUpper();
            keyword = keyword.ToUpper();

            string result = "";

            int keyword_index = 0;

            foreach (char symbol in input)
            {
                int c = (Array.IndexOf(characters, symbol) + Array.IndexOf(characters, keyword[keyword_index])) % N;

                result += characters[c];

                keyword_index++;

                if ((keyword_index + 1) == keyword.Length)
                    keyword_index = 0;
            }

            return result;
        }

        public string GenerateKeyWord(int length, int startSeed)
        {
            Random rand = new Random(startSeed);

            string result = "";

            for (int i = 0; i < length; i++)
                result += characters[rand.Next(0, characters.Length)];

            return result;
        }
    }


    public class key
    {
        private string val, dir;
        public string Val
        {
            get { return val; }
            set { val = value; }
        }

        public string Dir
        {
            get { return dir; }
            set { dir = value; }
        }
    }

    class FileReader //класс для работы с носителями информации (жесткий диск - файл) работа с жестким диском - путем открытия файла.
    {
        const uint GENERIC_READ = 0x80000000; //для чтения
        const uint GENERIC_WRITE = 0x40000000; //для записи
        const uint OPEN_EXISTING = 3; //тип открытия файла
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        System.IntPtr handle; //дескриптор окна
        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        //Kernel32.dll - динамически подключаемая библиотека, предоставляющая приложениям многие базовые функции
        //API Win32, в частности: управление памятью, операции ввода/вывода, создание процессов и потоков и функции синхронизации.
        //System.Runtime.InteropServices - пространство имен
        //DllImport - вызов неуправляемого кода из управляемого приложения
        //"kernel32" - библиотека
        //SetLastError - значение true, чтобы показать, что вызывающий объект вызовет SetLastError
        //ThrowOnUnmappableChar - исключение каждый раз Interop маршалер встречает unmappable характер
        //CharSet = System.Runtime.InteropServices.CharSet.Ansi - Указывает, какой набор знаков должны использовать маршалированные строки
        unsafe static extern System.IntPtr CreateFile
        (
        string FileName, // имя файла
        uint DesiredAccess, // режим доступа
        uint ShareMode, // режим 
        uint SecurityAttributes, // атрибуты безопасности
        uint CreationDisposition, // как создать
        uint FlagsAndAttributes, // атрибуты файла 
        int hTemplateFile // дескриптор файла шаблона
        );
        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        unsafe static extern bool ReadFile
        (
        System.IntPtr hFile, // дескриптор файла
        void* pBuffer, // буфер данных
        int NumberOfBytesToRead, // количество байт для чтения
        int* pNumberOfBytesRead, // количество прочитанных байт 
        int Overlapped // перекрывающий буфер
        );
        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        unsafe static extern bool WriteFile
        (
        System.IntPtr hFile, // дескриптор файла
        void* lpBuffer, // буфер данных 
        int NumberOfBytesToRead, // количество байт для чтения
        int* pNumberOfBytesRead, // количество прочитанных байт 
        int Overlapped // перекрывающийся буфер
        );
        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        unsafe static extern bool CloseHandle
        (
        System.IntPtr hObject // обращение к объекту
        );
        public bool OpenRead(string FileName) //работа с нулевым сектором
        {
            // открытие существующего файла для чтения 
            handle = CreateFile(FileName, GENERIC_READ, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0); //в дискрипторе окна вызывается ф-ия создания файла с параметрами чтения и открытия
                                                                                                  //параметры открытия
            if (handle != System.IntPtr.Zero) //если файл существует, озвращаем T, нет - F
                                              //IntPtr - нулевое поле
            {
                return true; //устройство найдено
            }
            else
            {
                return false; //устройство не надено
            }
        }
        public bool OpenWrite(string FileName)
        {
            // открытие существующего файла для записи 
            handle = CreateFile(FileName, GENERIC_WRITE, FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0); //параметры открытия
                                                                                                    //handle - дескриптор окна
            if (handle != System.IntPtr.Zero)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        unsafe public int Read(byte[] buffer, int count) //чтение ячеек flash
                                                         //сборщик мусора среды CLR может 
                                                         //Во избежание этого блок fixed используется для получения указателя на память и его пометки таким образом,
                                                         //что его не смог переместить сборщик мусора. В конце блока fixed память снова становится доступной для перемещения путем сборки мусора.
                                                         //Эта способность называется декларативным закреплением.
        {
            int n = 0;
            fixed (byte* p = buffer)
            {
                if (!ReadFile(handle, p, count, &n, 0)) //параметры чтения
                {
                    return 0;
                }
            }
            return n;
        }
        unsafe public int Write(byte[] buffer, int index, int count) //запись ячеек flash
        {
            int n = 0;
            fixed (byte* p = buffer) //Оператор fixed задает указатель на управляемую переменную
                                     //и "закрепляет" эту переменную во время выполнения оператора.
            {
                if (!WriteFile(handle, p + index, 512, &n, 0)) //параметры записи
                {
                    return 0;
                }
            }
            return n;
        }
        public bool Close()
        //bool - используется для объявления переменных для хранения логических значений, true и false.
        {
            return CloseHandle(handle); //закрыть дискриптор откна
        }
    }
}

