using System;
using System.IO;

namespace Artem
{
    class ShiftRegister
    {
        public long polinom;
        public long state;
        public long mask;
        public int sizeRegister;

        public ShiftRegister(string digit = "40035532523", int size = 32)  //40035532523  //435
        {
            long num = 0;
            sizeRegister = size;
            mask = Convert.ToInt64(Math.Pow(2, sizeRegister));
            num = Convert.ToInt64(digit, 8);
            polinom = num ^ mask;
            state = GenState(sizeRegister);
        }

        public void GetInfo()
        {
            Console.WriteLine("Polinom: {0} | {1}", polinom, Convert.ToString(polinom, 2).PadLeft(sizeRegister, '0'));
            Console.WriteLine("State {0} | {1}", state, Convert.ToString(state, 2).PadLeft(sizeRegister, '0'));
            Console.WriteLine("Mask: {0} | {1}", mask, Convert.ToString(mask, 2).PadLeft(sizeRegister, '0'));
            // Console.WriteLine("Period: {0}", countPeriod());

        }

        public long GenState(int shiftTimes)
        {
            long temp = 0;
            temp = polinom << (shiftTimes / 2);
            return temp % mask;
        }

        public long CountPeriod()
        {
            long check = mask >> 1;
            long temp = state;
            long firstDigit = 0;
            long count = 0;

            while (firstDigit != temp)
            {

                if (count == 1)
                {
                    firstDigit = temp;
                }

                if ((temp & check) == check)
                {
                    temp = (temp << 1) ^ mask;
                    temp ^= polinom;
                }
                else
                {
                    temp <<= 1;
                }

                count++;
            }

            return count;
        }

        public int GetBit()
        {
            long check = mask >> 1;
            if ((state & check) == check)
            {
                state = (state << 1) ^ mask;
                state ^= polinom;
                return 1;
            }
            else
            {
                state <<= 1;
                return 0;
            }
        }
        public long GetState()
        {
            GetBit();
            return state;
        }
    }
    class Program
    {
        static string path = Directory.GetCurrentDirectory();
        static string imageFile = "Ze.jpg";
        static string firstImage = "firstImage.jpg";
        static string secondImage = "secondImage.jpg";
        static string thirdImage = "thirdImage.jpg";
        static string decriptImage = "decriptImage.jpg";
        static int[] key_1 = { 1, 0, 1, 0 };
        static int[] key_2;
        static int[] min;
        static int[] max;
        static bool check = false;
        static int counter = 0;

        static void FindMinMax(int fileLength, int registerLength = 4)   // ĳ����� ����� �� max �� min
        {
            key_2 = new int[registerLength];
            min = new int[registerLength];
            max = new int[registerLength];

            int part = fileLength / registerLength;
            int temp = 0;

            for (int i = 0; i < registerLength; i++)
            {
                min[i] = temp;
                max[i] = min[i] + part;
                temp = max[i] + 1;

                if (i == registerLength - 1)
                    max[i] = fileLength - 1;

                key_2[i] = (key_1[i] == 0) ? min[i] : max[i];
            }

        }

        static int ConfusionData(int code)  // ����������� �����
        {

            int operation = key_1[code];
            int index = 0;
            check = false;

            if (operation == 0)
            {
                if (key_2[code] <= max[code])
                {
                    index = key_2[code];
                    key_2[code]++;
                    ++counter;
                    return index;
                }
            }
            else
            {
                if (key_2[code] >= min[code])
                {
                    index = key_2[code];
                    key_2[code]--;
                    ++counter;
                    return index;
                }
            }
            check = true;
            return 1;
        }

        static void FileStream(int mode = 1)
        {
            string pathOrigin = path + "\\" + imageFile;
            string pathFirstFile = path + "\\" + firstImage;
            string pathSecondFile = path + "\\" + secondImage;
            string pathThirdFile = path + "\\" + thirdImage;
            string pathDecript = path + "\\" + decriptImage;
            string[] pathImages = { pathFirstFile, pathSecondFile, pathThirdFile };
            byte[] resolutionImage = new byte[4];
            byte[] fullHeader;
            byte[] imageDataCrypt;
            uint buf = 0;
            byte[] imageHeader;


            uint[] indexHeader = { 0xffd8, 0xffda, 0xffc0 };       // size image, without head, width, height
            uint sizeHeader = 0;

            FileStream firstStream;

            firstStream = new FileStream(pathOrigin, FileMode.Open);
            byte[] dataImage = new byte[firstStream.Length];
            firstStream.Read(dataImage, 0, (int)firstStream.Length);

            for (uint i = 0; i < firstStream.Length; i++)
            {
                buf = ((uint)dataImage[i] << 8) | (dataImage[i + 1]);
                //Console.WriteLine("first {0} second {1}", dataImage[i], dataImage[i+1]);
                //Console.WriteLine("Buffer "+ buf);
                //Console.WriteLine(i);
                if (buf == indexHeader[1])
                {
                    //Console.WriteLine("Index "+i);
                    sizeHeader = i;
                    break;
                }
                //Console.WriteLine(i);

                //++i;
            }

            byte[] dataHeader = new byte[sizeHeader];
            byte[] bodyImage = new byte[firstStream.Length - sizeHeader];

            uint j = 0;
            for (uint i = 0; i < firstStream.Length; i++)
            {
                if (i < sizeHeader)
                {
                    dataHeader[i] = dataImage[i];

                }
                else
                {
                    bodyImage[j] = dataImage[i];
                    j++;

                }

            }


            uint indexResolution = 0;
            for (uint i = 0; i < dataHeader.Length; i++)
            {
                buf = ((uint)dataImage[i] << 8) | (dataImage[i + 1]);

                if (buf == indexHeader[2])
                {
                    //Console.WriteLine("Index "+i);
                    i = i + 5;
                    indexResolution = i; // remember index 
                    Array.Copy(dataHeader, i, resolutionImage, 0, 4);
                    break;
                }
            }


            uint height = ((uint)resolutionImage[0] << 8) | resolutionImage[1];
            uint width = ((uint)resolutionImage[2] << 8) | resolutionImage[3];

            uint heightBy3 = height / 3;
            uint widthBy3 = width / 3;

            resolutionImage[0] = Convert.ToByte((heightBy3 >> 8) % 256);
            resolutionImage[1] = Convert.ToByte(heightBy3 % 256);
            resolutionImage[2] = Convert.ToByte((widthBy3 >> 8) % 256);
            resolutionImage[3] = Convert.ToByte(widthBy3 % 256);
            Array.Copy(resolutionImage, 0, dataHeader, indexResolution, 4);

            ShiftRegister shift_32 = new ShiftRegister();
            ShiftRegister shift_8 = new ShiftRegister("435");
            FindMinMax(bodyImage.Length);

            byte[] cryptBodyImage = new byte[bodyImage.Length];
            int index = 0;
            while (bodyImage.Length != (counter + 1))
            {
                int firstBit = shift_32.GetBit() ^ shift_8.GetBit();
                int secondBit = shift_32.GetBit() ^ shift_8.GetBit();
                int generatedBit = Convert.ToInt32(firstBit + "" + secondBit, 2);

                index = ConfusionData(generatedBit);

                if (check == true)
                {
                    continue;
                }

                cryptBodyImage[counter] = bodyImage[index];

            }


            FileStream secondStream;
            int partImage = bodyImage.Length / 3;

            for (int i = 0; i < pathImages.Length; i++)
            {
                secondStream = new FileStream(pathImages[i], FileMode.Create);
                secondStream.Seek(0, SeekOrigin.Begin);
                secondStream.Write(dataHeader, 0, dataHeader.Length);
                secondStream.Write(cryptBodyImage, i * partImage, partImage);
                secondStream.Dispose();
            }


            //if (mode == 1)
            //{
            //    firstStream = new FileStream(pathOrigin, FileMode.Open);
            //}
            //else
            //{
            //    firstStream = new FileStream(pathFirstFile, FileMode.Open);
            //    multiply = 3;
            //}

            //imageHeaderSizes = GetHeaderInfo(firstStream, indexHeader, bytesRead);

            //headerSize = imageHeaderSizes[0] - imageHeaderSizes[1];
            //fullHeader = new byte[headerSize];
            //firstStream.Seek(0, SeekOrigin.Begin);
            //firstStream.Read(fullHeader, 0, headerSize); // ������� ������ ����������

            //byte[] imageData = new byte[imageHeaderSizes[1] * multiply];
            //imageDataCrypt = new byte[imageData.Length];

            //if (mode == 1)
            //{
            //    firstStream.Read(imageData, 0, imageData.Length); // ������� ����� ����������
            //    firstStream.Dispose();
            //}
            //else
            //{
            //    firstStream.Dispose();
            //    for (int i = 0; i < 3; i++)
            //    {
            //        firstStream = new FileStream(pathImages[i], FileMode.Open);
            //        firstStream.Seek(headerSize, SeekOrigin.Begin);
            //        firstStream.Read(imageData, i * imageHeaderSizes[1], imageHeaderSizes[1]); // ������� ����� ����������
            //        firstStream.Dispose();
            //    }

            //}

            //Console.WriteLine("\nImageData : " + imageData.Length);

            //ShiftRegister shift_32 = new ShiftRegister();
            //ShiftRegister shift_8 = new ShiftRegister("435", 8);

            //FindMinMax(imageHeaderSizes[1] * multiply);


            //while (imageData.Length != (counter + 1))
            //{
            //    int firstBit = shift_32.GetBit() ^ shift_8.GetBit();
            //    int secondBit = shift_32.GetBit() ^ shift_8.GetBit();
            //    int generatedBit = Convert.ToInt32(firstBit + "" + secondBit, 2);

            //    index = ConfusionData(generatedBit);

            //    if (check == true)
            //    {
            //        continue;
            //    }

            //    if (mode == 1)
            //    {
            //        imageDataCrypt[counter] = imageData[index];
            //    }
            //    else
            //    {
            //        imageDataCrypt[index] = imageData[counter];
            //    }
            //}


            //if (mode == 1)
            //{
            //    int partImage = imageDataCrypt.Length / 3;
            //    fullHeader = SetHeaderInfo(imageHeaderSizes, fullHeader, indexHeader);

            //    for (int i = 0; i < pathImages.Length; i++)
            //    {
            //        secondStream = new FileStream(pathImages[i], FileMode.Create);
            //        secondStream.Seek(0, SeekOrigin.Begin);
            //        secondStream.Write(fullHeader, 0, fullHeader.Length);
            //        secondStream.Write(imageDataCrypt, i * partImage, partImage);

            //        secondStream.Dispose();
            //    }
            //}
            //else
            //{
            //    fullHeader = SetHeaderInfo(imageHeaderSizes, fullHeader, indexHeader, 0);

            //    secondStream = new FileStream(pathDecript, FileMode.Create);
            //    secondStream.Seek(0, SeekOrigin.Begin);
            //    secondStream.Write(fullHeader, 0, fullHeader.Length);
            //    secondStream.Write(imageDataCrypt, 0, imageDataCrypt.Length);
            //    secondStream.Dispose();
            //}



        }
        static void Main(string[] args)
        {
            FileStream();
            Console.ReadLine();

        }
    }
}
