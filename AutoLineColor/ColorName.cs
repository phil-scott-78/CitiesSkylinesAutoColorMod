using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace AutoLineColor
{
    class ColorName
    {
        public ColorName(string fullColorDefinition)
        {
            try
            {
                Color = HexToColor(fullColorDefinition.Trim().Substring(0, 7));
                Name = fullColorDefinition.Trim().Substring(8).Trim();
            }
            catch (Exception ex)
            {
                Helper.PrintValue(ex.Message);
            }

        }

        public static Dictionary<TransportInfo.TransportType, ColorName[]> BuildColorMap()
        {
            const string blueColors = @"#98AFC7	Gray
                                        #657383	Slate
                                        #616D7E	Jet
                                        #646D7E	Mist
                                        #566D7E	Marble
                                        #4863A0	Steel
                                        #2B547E	Blue Jay
                                        #151B54	Midnight
                                        #000080	Navy
                                        #342D7E	Whale
                                        #15317E	Lapis
                                        #151B8D	Cornflower
                                        #0000A0	Earth
                                        #0020C2	Cobalt
                                        #0041C2	Blueberry
                                        #2554C7	Sapphire
                                        #1569C7	Blue-Eye
                                        #2B60DE	Royal
                                        #1F45FC	Orchid
                                        #6960EC	Lotus
                                        #368BC1	Glacial
                                        #488AC7	Silk
                                        #3090C7	Ivy
                                        #659EC7	Koi
                                        #87AFC7	Columbia
                                        #95B9C7	Baby Blue
                                        #2B65EC	Ocean
                                        #306EFF	Blue Ribbon
                                        #1589FF	Dodger
                                        #6698FF	Sky
                                        #38ACEC	Butterfly
                                        #56A5EC	Iceberg
                                        #5CB3FF	Crystal
                                        #79BAEC	Denim";

            const string greenColors = @"#4C787E Beetle
                                        #008080	Teal
                                        #4E8975	Sea
                                        #78866B	Camouflage
                                        #848b79	Sage 
                                        #617C58	Hazel 
                                        #728C00	Venom 
                                        #667C26	Fern 
                                        #254117	Forrest 
                                        #437C17	Seaweed 
                                        #387C44	Pine 
                                        #347C2C	Jungle 
                                        #347C17	Shamrock 
                                        #348017	Spring 
                                        #4AA02C	Spring 
                                        #41A317	Lime 
                                        #3EA055	Clover 
                                        #6CBB3C	Snake
                                        #6CC417	Alien 
                                        #4CC417	Green Apple
                                        #4CC552	Kelly 
                                        #54C571	Zombie 
                                        #99C68E	Frog 
                                        #89C35C	Pea
                                        #85BB65	Dollar Bill
                                        #9CB071	Iguana 
                                        #B2C248	Avocado 
                                        #9DC209	Pistachio 
                                        #A1C935	Salad 
                                        #7FE817	Hummingbird 
                                        #59E817	Nebula 
                                        #57E964	Go 
                                        #64E986	Algae 
                                        #5EFB6E	Jade 
                                        #00FF00	Green
                                        #5FFB17	Emerald 
                                        #87F717	Lawn 
                                        #8AFB17	Chartreuse
                                        #6AFB92	Dragon 
                                        #98FF98	Mint 
                                        #CCFB5D	Tea 
                                        #BCE954	Slime ";
            const string orangeColors = @"#C47451	Salmon
                                        #C36241	Rust
                                        #C35817	Red Fox
                                        #C85A17	Chocolate
                                        #CC6600	Sedona
                                        #E56717	Papaya 
                                        #E66C2C	Halloween 
                                        #F87217	Pumpkin 
                                        #F87431	Construction Cone 
                                        #E67451	Sunrise 
                                        #FF8040	Mango 
                                        #F88017	Dark ";
            var colorMap = new Dictionary<TransportInfo.TransportType, ColorName[]>
            {
                {
                    TransportInfo.TransportType.Bus,
                    blueColors.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(i => new ColorName(i))
                        .ToArray()
                },
                {
                    TransportInfo.TransportType.Metro,
                    greenColors.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(i => new ColorName(i))
                        .ToArray()
                },
                {
                    TransportInfo.TransportType.Train,
                    orangeColors.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(i => new ColorName(i))
                        .ToArray()
                }
            };
            return colorMap;
        }

        public string Name { get; set; }
        public Color32 Color { get; set; }

        private static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", ""); //in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", ""); //in case the string is formatted #FFFFFF

            byte alpha = 255; //assume fully visible unless specified in hex

            var red = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var green = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var blue = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                alpha = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            }

            return new Color32(red, green, blue, alpha);
        }
    }
}