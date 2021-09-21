﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using StbRectPackSharp;

namespace ClassicUO.Renderer
{
    class TextureAtlas : IDisposable
    {
        private readonly int _width, _height;
        private readonly SurfaceFormat _format;
        private readonly GraphicsDevice _device;
        private readonly List<Texture2D> _textureList;
        private Packer _packer;
        private readonly Rectangle[] _spriteBounds;
        private readonly byte[] _spriteTextureIndices;


        public static TextureAtlas Shared { get; }

        static TextureAtlas()
        {
            const int TEXTURE_SIZE = 1024 * 4;

            Shared = new TextureAtlas 
            (
                Client.Game.GraphicsDevice,
                TEXTURE_SIZE,
                TEXTURE_SIZE,
                SurfaceFormat.Color,
                ushort.MaxValue * 2
            );
        }


        public TextureAtlas(GraphicsDevice device, int width, int height, SurfaceFormat format, int maxSpriteCount)
        {
            _device = device;
            _width = width;
            _height= height;
            _format = format;

            _textureList = new List<Texture2D>();
            _spriteBounds = new Rectangle[maxSpriteCount];
            _spriteTextureIndices = new byte[maxSpriteCount];
            _spriteTextureIndices.AsSpan().Fill(0xFF);
        }


        public int TexturesCount => _textureList.Count;


        public unsafe void AddSprite<T>(uint hash, Span<T> pixels, int width, int height) where T : unmanaged
        {
            if (IsHashExists(hash))
            {
                return;
            }

            var index = _textureList.Count - 1;

            if (index < 0)
            {
                index = 0;
                CreateNewTexture2D();
            }

            ref Rectangle pr = ref _spriteBounds[hash];
            while (!_packer.PackRect(width, height, out pr))
            {
                CreateNewTexture2D();
                index = _textureList.Count - 1;
            }

            Texture2D texture = _textureList[index];

            fixed (T* src = pixels)
            {
                texture.SetDataPointerEXT
                (
                    0,
                    pr,
                    (IntPtr)src,
                    sizeof(T) * pixels.Length
                );
            }

            _spriteTextureIndices[hash] = (byte) index;
        }

        private void CreateNewTexture2D()
        {
            Texture2D texture = new Texture2D(_device, _width, _height, false, _format);
            _textureList.Add(texture);

            _packer?.Dispose();
            _packer = new Packer(_width, _height);
        }

        public Texture2D GetTexture(uint hash, out Rectangle bounds)
        {
            if (IsHashExists(hash))
            {
                bounds = _spriteBounds[(int)hash];
              
                return _textureList[_spriteTextureIndices[(int)hash]];
            }

            bounds = Rectangle.Empty;
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHashExists(uint hash) => _spriteTextureIndices[(int)hash] != 0xFF;

        public void SaveImages(string name)
        {
            for (int i = 0, count = TexturesCount; i < count; ++i)
            {
                var texture = _textureList[i];

                using (var stream = System.IO.File.Create($"atlas/{name}_atlas_{i}.png"))
                {
                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                }
            }
        }

        public void Dispose()
        {
            foreach (Texture2D texture in _textureList)
            {
                if (!texture.IsDisposed)
                {
                    texture.Dispose();
                }
            }

            _packer.Dispose();
            _textureList.Clear();
            _spriteTextureIndices.AsSpan().Fill(0xFF);
        }
    }
}
