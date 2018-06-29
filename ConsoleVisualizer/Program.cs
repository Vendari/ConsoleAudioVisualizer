﻿using System;
using System.Text;
using NAudio.Wave; // installed with nuget
using System.Numerics;
using System.Threading;
using System.Linq;

namespace ConsoleVisualizer

{
    class Program
    {
        #region settings
        static int sensivity = 10;
        static int maxValue = 40; //max height
        static string symbol = "#"; //symbol or text i visualizer
        static int countOfLines = 40; // 1 to 51
        #endregion

        static BufferedWaveProvider bwp;
        static StringBuilder sb = new StringBuilder();

        static int RATE = 48000; // rate of the sound card
        static int BUFFERSIZE = (int)Math.Pow(2, 11); // must be a multiple of 2

        static void Main(string[] args)
        {
            int devcount = WaveIn.DeviceCount;

            var wi = new WaveInEvent();
            wi.DeviceNumber = 0;
            wi.WaveFormat = new NAudio.Wave.WaveFormat(RATE, 1);
            wi.BufferMilliseconds = (int)((double)BUFFERSIZE / (double)RATE * 1000.0);

            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
            bwp = new BufferedWaveProvider(wi.WaveFormat);
            bwp.BufferLength = BUFFERSIZE * 2;
            bwp.DiscardOnBufferOverflow = true;
            wi.StartRecording();
            int left = Console.WindowLeft, top = Console.WindowTop;
            for (int i=0; ;i++)
            {
                UpdateAudioGraph();
                Console.SetCursorPosition(0, 0);
                Console.Clear();
                Console.Write(sb.ToString());
                sb.Clear();
                Console.SetCursorPosition(0, 0);
                if (i % 30 ==0)
                {
                    Random rnd = new Random();
                    Console.ForegroundColor = (ConsoleColor)rnd.Next(1, 15);
                    i = 0;
                }
            }
        }

        static void UpdateAudioGraph()
        {
            int frameSize = BUFFERSIZE;
            var frames = new byte[frameSize];
            bwp.Read(frames, 0, frameSize);
            if (frames.Length == 0) return;
            if (frames[frameSize - 2] == 0) return;

            int SAMPLE_RESOLUTION = 16;
            int BYTES_PER_POINT = SAMPLE_RESOLUTION / 8;
            Int32[] vals = new Int32[frames.Length / BYTES_PER_POINT];
            double[] Ys = new double[frames.Length / BYTES_PER_POINT];
            double[] Ys2 = new double[frames.Length / BYTES_PER_POINT];
            int counter = 0, n = 0;
            for (int i = 0; i < vals.Length; i++)
            {
                byte hByte = frames[i * 2 + 1];
                byte lByte = frames[i * 2 + 0];
                vals[i] = (int)(short)((hByte << 8) | lByte);
                Ys[i] = vals[i];

                Ys2 = FFT(Ys);
                Ys2.Take(Ys2.Length / 2).ToArray();

                if (counter % 20 == 0 && n<=countOfLines)
                {
                    n++;
                    DrawGraph(Ys2[i]);
                }
                counter++;
            }
        }

        static void DrawGraph(double Height)
        {
            int n = 0;
            for (int i = 0; i < Math.Abs(Height *sensivity); i++)
            {
                if (n > maxValue)
                    break;
                sb.Append(symbol);
                n++;
            }
            sb.Append('\n');
        }

        static void wi_DataAvailable(object sender, WaveInEventArgs e)

        {
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        static double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length]; // this is where we will store the output (fft)
            Complex[] fftComplex = new Complex[data.Length]; // the FFT function requires complex format
            for (int i = 0; i < data.Length; i++)
            {
                fftComplex[i] = new Complex(data[i], 0.0); // make it complex format (imaginary = 0)
            }

            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);

            for (int i = 0; i < data.Length; i++)
            {
                fft[i] = fftComplex[i].Magnitude; // back to double
                //fft[i] = Math.Log10(fft[i]); // convert to dB
            }

            return fft;
            //todo: this could be much faster by reusing variables
        }
    }
}
