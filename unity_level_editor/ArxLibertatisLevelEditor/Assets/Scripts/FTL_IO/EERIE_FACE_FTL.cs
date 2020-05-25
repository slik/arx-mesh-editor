﻿using Assets.Scripts.Shared_IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.FTL_IO
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EERIE_FACE_FTL
    {
        public int facetype; // 0 = flat, 1 = text, 2 = Double-Side
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] rgb;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] vid;
        public short texid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] u;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] v;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public short[] ou;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public short[] ov;
        public float transval;
        public SavedVec3 norm;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public SavedVec3[] nrmls;
        public float temp;
    }
}
