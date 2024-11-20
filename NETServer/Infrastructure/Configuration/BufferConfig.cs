﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NETServer.Infrastructure.Configuration
{
    internal class BufferConfig
    {
        public static readonly int TotalBuffers = 100;
        public readonly static Dictionary<int, double> BufferAllocations = new()
        {
            { 128, 0.03 },
            { 256, 0.12 },
            { 512, 0.15 },
            { 1024, 0.35 },
            { 2048, 0.10 },
            { 4096, 0.20 },
            { 8192, 0.05 }
        };
}
}