using System.Collections.Generic;
using System.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;


using CoreBoy.memory;

using System.IO;

using static Pokemon.RedBlue;
using static Pokemon.RedBlue.MemoryAdresses;

namespace TwitterBoy
{
    public static class Header
    {
        struct Pokemon
        {
            int Species, Level, MaxHp, CurrentHp;
            string Name, Status; 
            public Pokemon(int index)
            {
                Name = GetPokemonNickname(index);
                Species = GetPokemonSpecies(index);
                Level = GetPokemonLevel(index);
                MaxHp = GetPokemonMaxHP(index);
                CurrentHp = GetPokemonCurrentHP(index);
                Status = GetPokemonStatus(index);
            }

            public void Draw(Image image, int index)
            {
                if(Species < 1 || Species > 151) return;
                 
                var startPosition = new Point(index * 250, 100);
                var textPosition  = new Point(index * 250, 200);

                var sprite = Image.Load(Paths.Sprites + Species + ".png");
                
                sprite.Mutate(x => x.Resize(sprite.Width * 4, sprite.Height * 4, KnownResamplers.NearestNeighbor));
                var spriteOffset = new Point((250 - sprite.Width) / 2, 0);

                var n = Name;
                var l = Level;
                var s = Status;
                var m = MaxHp;
                var h = CurrentHp;

                image.Mutate(x => x.DrawImage(sprite, new Point(startPosition.X + spriteOffset.X, startPosition.Y + spriteOffset.Y), 1));

                image.Mutate(x => x.DrawText(n, nameFont, Color.Black, new PointF(textPosition.X + 40, textPosition.Y + 160)));
                image.Mutate(x => x.DrawText("Lvl:" + l, statsFont, Color.Black, new PointF(textPosition.X + 40, textPosition.Y + 200)));
                image.Mutate(x => x.DrawText("HP:" + h + "/" + m, statsFont, Color.Black, new PointF(textPosition.X + 40, textPosition.Y + 220)));
                image.Mutate(x => x.DrawText(s, statsFont, Color.Black, new PointF(textPosition.X + 40, textPosition.Y + 240)));
            }
        }

        static int count;
        static string last;
        static FontFamily fontFamily;
        static Font nameFont, statsFont;
        static Mmu mmu;

        public static void Initialize(Mmu mmu)
        {
            Header.mmu = mmu;
            fontFamily = new FontCollection().Install(Paths.HeaderFont);
            nameFont  = new Font(fontFamily, 40);
            statsFont = new Font(fontFamily, 20);
        }

        public static string Update()
        {
            if(last != null && last != "") File.Delete(last);
            var path = Paths.OutputDirectory + "header_" + count + ".png";
            last = path;

            var image = new Image<Rgba32>(1500, 500, new Rgba32(1.0f, 1, 1));
            int pkmnCount = GetPokemonCount();

            for(int i = 0; i < pkmnCount; ++i)
            new Pokemon(i).Draw(image, i);

            image.Save(path);
            ++count;

            return path;
        }

        static string GetPokemonNickname(int index)
        {
            string output = "";
            for(int i = 0; i < PokemonNicknameLength; ++i)
            {
                var data = mmu.GetByte(PokemonNickname + i + index * 11);
                if(HexToASCII.ContainsKey(data)) output += HexToASCII[data];
                else break;
            }

            return output;
        }

        static int GetPokemonCount()
        {
            return mmu.GetByte(PartySize);
        }

        static int GetPokemonSpecies(int index)
        {
            return IDToPokedex[mmu.GetByte(PokemonSpecies + index * PokemonDataLength)];
        }

        static int GetPokemonMaxHP(int index)
        {
            return mmu.GetByte(PokemonMaxHP0 + index * PokemonDataLength) + mmu.GetByte(PokemonMaxHP1 + index * PokemonDataLength);
        }

        static int GetPokemonCurrentHP(int index)
        {
            return mmu.GetByte(PokemonCurrentHP0 + index * PokemonDataLength) + mmu.GetByte(PokemonCurrentHP1 + index * PokemonDataLength);
        }

        static int GetPokemonLevel(int index)
        {
            return mmu.GetByte(PokemonLevel + index * PokemonDataLength);
        }

        static string GetPokemonStatus(int index)
        {
            var data = mmu.GetByte(PokemonStatus + index * PokemonDataLength);
            if((data & 1 << 6) != 0) return "PARALYZED";
            if((data & 1 << 5) != 0) return "FROZEN";
            if((data & 1 << 4) != 0) return "BURNED";
            if((data & 1 << 3) != 0) return "POISONED";
            if((data & 1 << 2) != 0
            || (data & 1 << 1) != 0) return "ASLEEP";

            return "";
        }
    }
}