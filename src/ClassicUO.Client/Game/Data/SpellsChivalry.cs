#region license

// Copyright (c) 2024, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data
{
    internal static class SpellsChivalry
    {
        private static readonly Dictionary<int, SpellDefinition> _spellsDict;

        static SpellsChivalry()
        {
            _spellsDict = new Dictionary<int, SpellDefinition>
            {
                // Spell List
                {
                    1,
                    new SpellDefinition
                    (
                        "Purificazione del Fuoco",
                        201,
                        0x5100,
                        0x5100,
                        "Expor Flamus",
                        15,
                        35,
                        50,
                        TargetType.Beneficial,
                        Reagents.None
                    )
                },
                {
                    2,
                    new SpellDefinition
                    (
                        "Cicatrizzare",
                        202,
                        0x5101,
                        0x5101,
                        "Obsu Vulni",
                        15,
                        35,
                        50,
                        TargetType.Beneficial,
                        Reagents.None
                    )
                },
                {
                    3,
                    new SpellDefinition
                    (
                        "Consacrare l'Arma",
                        203,
                        0x5102,
                        0x5102,
                        "Consecrus Arma",
                        15,
                        45,
                        50,
                        TargetType.Neutral,
                        Reagents.None
                    )
                },
                {
                    4,
                    new SpellDefinition
                    (
                        "Disperdere il Male",
                        204,
                        0x5103,
                        0x5103,
                        "Dispiro Malas",
                        20,
                        55,
                        70,
                        TargetType.Neutral,
                        Reagents.None
                    )
                },
                {
                    5,
                    new SpellDefinition
                    (
                        "Furia Divina",
                        205,
                        0x5104,
                        0x5104,
                        "Divinum Furis",
                        15,
                        55,
                        70,
                        TargetType.Neutral,
                        Reagents.None
                    )
                },
                {
                    6,
                    new SpellDefinition
                    (
                        "Acerrimo Nemico",
                        206,
                        0x5105,
                        0x5105,
                        "Forul Solum",
                        20,
                        45,
                        70,
                        TargetType.Neutral,
                        Reagents.None
                    )
                },
                {
                    7,
                    new SpellDefinition
                    (
                        "Luce Sacra",
                        207,
                        0x5106,
                        0x5106,
                        "Augus Luminos",
                        20,
                        55,
                        70,
                        TargetType.Harmful,
                        Reagents.None
                    )
                },
                {
                    8,
                    new SpellDefinition
                    (
                        "Nobile Sacrificio",
                        208,
                        0x5107,
                        0x5107,
                        "Dium Prostra",
                        50,
                        65,
                        1000,
                        TargetType.Beneficial,
                        Reagents.None
                    )
                },
                {
                    9,
                    new SpellDefinition
                    (
                        "Rimuovere Maledizione",
                        209,
                        0x5108,
                        0x5108,
                        "Extermo Vomica",
                        20,
                        5,
                        50,
                        TargetType.Beneficial,
                        Reagents.None
                    )
                },
                {
                    10,
                    new SpellDefinition
                    (
                        "Viaggio Astrale",
                        210,
                        0x5109,
                        0x5109,
                        "Sanctum Viatas",
                        60,
                        100,
                        1000,
                        TargetType.Neutral,
                        Reagents.None
                    )
                }
            };
        }

        public static string SpellBookName { get; set; } = SpellBookType.Chivalry.ToString();

        public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;
        internal static int MaxSpellCount => _spellsDict.Count;

        public static SpellDefinition GetSpell(int spellIndex)
        {
            if (_spellsDict.TryGetValue(spellIndex, out SpellDefinition spell))
            {
                return spell;
            }

            return SpellDefinition.EmptySpell;
        }

        public static void SetSpell(int id, in SpellDefinition newspell)
        {
            _spellsDict[id] = newspell;
        }

        internal static void Clear()
        {
            _spellsDict.Clear();
        }
    }
}