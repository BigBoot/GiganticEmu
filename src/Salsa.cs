using System;
using System.IO;
using System.Text;

public class Salsa
{
    public int Rounds { get; private set; }

    private uint[] Matrix { get; set; }

    public Salsa(string key, int rounds)
    {
        Rounds = rounds;

        using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(key)))
        {
            using (var reader = new BinaryReader(stream))
            {
                var k = new uint[] { reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32() };
                var c = new uint[] { 0x61707865, 0x3120646e, 0x79622d36, 0x6b206574 };

                Matrix = new uint[]
                {
                    c[0], k[0], k[1], k[2],
                    k[3], c[1],    0,    0,
                       0,    0, c[2], k[0],
                    k[1], k[2], k[3], c[3],
                };
            }
        }
    }

    public string Decrypt(byte[] data) => Encoding.UTF8.GetString(Crypt(data));

    public byte[] Encrypt(string data) => Crypt(Encoding.UTF8.GetBytes(data));

    private byte[] Crypt(byte[] data)
    {
        byte[] output = new byte[data.Length];
        int bytesTransformed = 0;

        while (bytesTransformed < data.Length)
        {
            Salsa20Block(output, bytesTransformed);

            Matrix[8] = unchecked(Matrix[8] + 1);
            if (Matrix[8] == 0)
            {
                Matrix[9] = unchecked(Matrix[9] + 1);
            }

            int blockSize = Math.Min(64, data.Length - bytesTransformed);

            for (int i = 0; i < blockSize; i++)
            {
                output[bytesTransformed + i] = (byte)(data[bytesTransformed + i] ^ output[bytesTransformed + i]);
            }

            bytesTransformed += blockSize;
        }

        return output;
    }

    private void Salsa20Block(byte[] output, int offset)
    {
        uint[] x = (uint[])Matrix.Clone();

        for (var i = 0; i < Rounds; i += 2)
        {
            x[4] ^= Rotate((x[0] + x[12]) & 0xffffffff, 7);
            x[8] ^= Rotate((x[4] + x[0]) & 0xffffffff, 9);
            x[12] ^= Rotate((x[8] + x[4]) & 0xffffffff, 13);
            x[0] ^= Rotate((x[12] + x[8]) & 0xffffffff, 18);
            x[9] ^= Rotate((x[5] + x[1]) & 0xffffffff, 7);
            x[13] ^= Rotate((x[9] + x[5]) & 0xffffffff, 9);
            x[1] ^= Rotate((x[13] + x[9]) & 0xffffffff, 13);
            x[5] ^= Rotate((x[1] + x[13]) & 0xffffffff, 18);
            x[14] ^= Rotate((x[10] + x[6]) & 0xffffffff, 7);
            x[2] ^= Rotate((x[14] + x[10]) & 0xffffffff, 9);
            x[6] ^= Rotate((x[2] + x[14]) & 0xffffffff, 13);
            x[10] ^= Rotate((x[6] + x[2]) & 0xffffffff, 18);
            x[3] ^= Rotate((x[15] + x[11]) & 0xffffffff, 7);
            x[7] ^= Rotate((x[3] + x[15]) & 0xffffffff, 9);
            x[11] ^= Rotate((x[7] + x[3]) & 0xffffffff, 13);
            x[15] ^= Rotate((x[11] + x[7]) & 0xffffffff, 18);

            x[1] ^= Rotate((x[0] + x[3]) & 0xffffffff, 7);
            x[2] ^= Rotate((x[1] + x[0]) & 0xffffffff, 9);
            x[3] ^= Rotate((x[2] + x[1]) & 0xffffffff, 13);
            x[0] ^= Rotate((x[3] + x[2]) & 0xffffffff, 18);
            x[6] ^= Rotate((x[5] + x[4]) & 0xffffffff, 7);
            x[7] ^= Rotate((x[6] + x[5]) & 0xffffffff, 9);
            x[4] ^= Rotate((x[7] + x[6]) & 0xffffffff, 13);
            x[5] ^= Rotate((x[4] + x[7]) & 0xffffffff, 18);
            x[11] ^= Rotate((x[10] + x[9]) & 0xffffffff, 7);
            x[8] ^= Rotate((x[11] + x[10]) & 0xffffffff, 9);
            x[9] ^= Rotate((x[8] + x[11]) & 0xffffffff, 13);
            x[10] ^= Rotate((x[9] + x[8]) & 0xffffffff, 18);
            x[12] ^= Rotate((x[15] + x[14]) & 0xffffffff, 7);
            x[13] ^= Rotate((x[12] + x[15]) & 0xffffffff, 9);
            x[14] ^= Rotate((x[13] + x[12]) & 0xffffffff, 13);
            x[15] ^= Rotate((x[14] + x[13]) & 0xffffffff, 18);
        }


        for (var i = 0; i < 16; i++)
        {
            ToBytes((x[i] + Matrix[i]) & 0xffffffff, output, offset + i * 4, Math.Max(output.Length - offset - (4 * i), 0));
        }
    }

    /* Bit Twiddling methods */

    // Serialize the input integer into the output buffer. The input integer 
    // will be split into 4 bytes and put into four sequential places in the 
    // output buffer, starting at the outputOffset. 
    private static void ToBytes(uint input, byte[] output, int outputOffset, int size)
    {
        unchecked
        {
            if (size > 0) output[outputOffset] = (byte)input;
            if (size > 1) output[outputOffset + 1] = (byte)(input >> 8);
            if (size > 2) output[outputOffset + 2] = (byte)(input >> 16);
            if (size > 3) output[outputOffset + 3] = (byte)(input >> 24);
        }
    }

    private static uint Rotate(uint v, int c)
    {
        return (v << c) | (v >> (32 - c));
    }
}