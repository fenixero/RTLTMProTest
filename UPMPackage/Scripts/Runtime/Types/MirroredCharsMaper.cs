using System.Collections.Generic;

// The SeedV Lab (Beijing SeedV Technology Co., Ltd.) modifications: Copyright 2024 The SeedV Lab
// (Beijing SeedV Technology Co., Ltd.) All Rights Reserved. The modifications in this file are the intellectual property of the SeedV Lab.

namespace RTLTMPro {
  public class MirroredCharsMaper {
    public static readonly Dictionary<char, char> MirroredCharsMap = new() {
      { '(', ')' },
      { ')', '(' },
      { '[', ']' },
      { ']', '[' },
      { '{', '}' },
      { '}', '{' },
      { '<', '>' },
      { '>', '<' },
      { '«', '»' },
      { '»', '«' },
      { '「', '」' },
      { '」', '「' },
      { '『', '』' },
      { '』', '『' },
      { '〚', '〛' },
      { '〛', '〚' },
      { '‘', '’' },
      { '’', '‘' },
      { '“', '”' },
      { '”', '“' },
      { '⦗', '⦘' },
      { '⦘', '⦗' },
      { '⟨', '⟩' },
      { '⟩', '⟨' },
      { '⦃', '⦄' },
      { '⦄', '⦃' },
      { '⟦', '⟧' },
      { '⟧', '⟦' },
      { '《', '》' },
      { '》', '《' },
      { '❨', '❩' },
      { '❩', '❨' },
      { '⸨', '⸩' },
      { '⸩', '⸨' },
      { '⌈', '⌉' },
      { '⌉', '⌈' },
      { '⌊', '⌋' },
      { '⌋', '⌊' },
      { '｢', '｣' },
      { '｣', '｢' },
      { '❲', '❳' },
      { '❳', '❲' }
    };
  }
}
