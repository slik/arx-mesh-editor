﻿using Assets.Scripts.Util;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.ArxNative.IO
{
    public static class ArxIOHelper
    {
        public static string GetString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(ArrayHelper.TrimEnd<byte>(bytes, 0));
        }

        public static byte[] GetBytes(string str, int bytesLength)
        {
            return ArrayHelper.FixArrayLength<byte>(Encoding.ASCII.GetBytes(str), bytesLength, 0);
        }

        public static Color FromBGRA(uint bgra)
        {
            //return Color.red;

            byte[] bytes = BitConverter.GetBytes(bgra);
            return new Color(bytes[2] / 255f, bytes[1] / 255f, bytes[0] / 255f);
        }

        public static Color FromRGB(uint rgba)
        {
            byte[] bytes = BitConverter.GetBytes(rgba);
            return new Color(bytes[0] / 255f, bytes[1] / 255f, bytes[2] / 255f);
        }

        public static string ArxPathToPlatformPath(string arxPath)
        {
            string[] parts = arxPath.Split('\\');
            return Path.Combine(parts);
        }

        public static string PlatformPathToArxPath(string platformPath)
        {
            string[] parts = platformPath.Split(Path.DirectorySeparatorChar);
            return string.Join("\\", parts);
        }

        public static int XZToCellIndex(int x, int z, int sizex, int sizez)
        {
            return z * sizex + x;
        }

        public static uint ToBGRA(Color color)
        {
            byte[] bytes = new byte[]
            {
                (byte)(color.b*255),
                (byte)(color.g*255),
                (byte)(color.r*255),
                (byte)(color.a*255),
            };

            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
