namespace TabulateAI.Helpers;

public sealed record MerchantBrand(string BrandKey, string Domain, string[] Keywords);

public static class MerchantLogoRegistry
{
    private static readonly MerchantBrand[] Brands =
    [
        new("woolworths", "woolworths.com.au", ["woolworths", "woolies"]),
        new("coles", "coles.com.au", ["coles"]),
        new("aldi", "aldi.com.au", ["aldi"]),
        new("iga", "iga.com.au", [" iga", "iga "]),
        new("costco", "costco.com.au", ["costco", "cosco"]),
        new("harris-farm", "harrisfarm.com.au", ["harris farm"]),
        new("bunnings", "bunnings.com.au", ["bunnings"]),
        new("kmart", "kmart.com.au", ["kmart", "k mart"]),
        new("target", "target.com.au", ["target"]),
        new("big-w", "bigw.com.au", ["big w", "bigw"]),
        new("myer", "myer.com.au", ["myer"]),
        new("david-jones", "davidjones.com", ["david jones"]),
        new("jb-hi-fi", "jbhifi.com.au", ["jb hi", "jbhifi"]),
        new("harvey-norman", "harveynorman.com.au", ["harvey norman"]),
        new("eb-games", "ebgames.com.au", ["eb games", "ebgames"]),
        new("apple", "apple.com", ["apple store", " apple "]),
        new("supercheap-auto", "supercheapauto.com.au", ["supercheap auto", "supercheap"]),
        new("officeworks", "officeworks.com.au", ["officeworks"]),
        new("chemist-warehouse", "chemistwarehouse.com.au", ["chemist warehouse"]),
        new("priceline", "priceline.com.au", ["priceline"]),
        new("shell", "shell.com.au", ["shell"]),
        new("bp", "bp.com", ["bp express", "bp connect", "bp "]),
        new("ampol", "ampol.com.au", ["ampol", "caltex"]),
        new("uber-eats", "ubereats.com", ["uber eats", "ubereats"]),
        new("uber", "uber.com", ["uber"]),
        new("mcdonalds", "mcdonalds.com", ["mcdonald", "maccas"]),
        new("kfc", "kfc.com.au", ["kfc"]),
        new("hungry-jacks", "hungryjacks.com.au", ["hungry jack", "hungry jacks"]),
        new("guzman-y-gomez", "gyg.com.au", ["guzman", "gyg"]),
        new("subway", "subway.com", ["subway"]),
        new("starbucks", "starbucks.com.au", ["starbucks"]),
        new("7-eleven", "7eleven.com.au", ["7-eleven", "7 eleven"]),
        new("netflix", "netflix.com", ["netflix"]),
        new("spotify", "spotify.com", ["spotify"]),
        new("amazon", "amazon.com.au", ["amazon"]),
        new("hoyts", "hoyts.com.au", ["hoyts"]),
        new("telstra", "telstra.com.au", ["telstra"]),
        new("optus", "optus.com.au", ["optus"]),
    ];

    public static MerchantBrand? Match(string merchant)
    {
        if (string.IsNullOrWhiteSpace(merchant))
        {
            return null;
        }

        var normalized = $" {merchant.ToLowerInvariant()} ";

        foreach (var brand in Brands)
        {
            if (brand.BrandKey == "uber" &&
                (normalized.Contains("uber eats", StringComparison.OrdinalIgnoreCase) ||
                 normalized.Contains("ubereats", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (brand.Keywords.Any(keyword =>
                    normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return brand;
            }
        }

        return null;
    }
}
