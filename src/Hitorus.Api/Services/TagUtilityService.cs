﻿using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Hitorus.Api.Services;
public partial class TagUtilityService(HttpClient httpClient, IDbContextFactory<HitomiContext> dbContextFactory) {
    [GeneratedRegex("""<div class="content">(.+?)</div>""")]
    private static partial Regex HtmlContentRegex();
    [GeneratedRegex("""<a href="[^"]+">(.+?)</a> \((\d+)\)""")]
    private static partial Regex TagInfoRegex();

    /// <summary>
    /// {0} is the category string.
    /// {1} is the first character of the tag or 123.
    /// </summary>
    private const string NON_MFT_TAG_DATA_HTML = "https://hitomi.la/all{0}-{1}.html";
    /// <summary>
    /// {0} is the first character of the tag or 123.
    /// </summary>
    private const string MFT_TAG_DATA_HTML = "https://hitomi.la/alltags-{0}.html";
    private const char MALE_SYMBOL = '♂';
    private const char FEMALE_SYMBOL = '♀';

    public async Task FetchUpdateNonMFTTags(TagCategory category, IEnumerable<TagDTO> tags) {
        if (category is TagCategory.Male or TagCategory.Female or TagCategory.Tag) {
            throw new ArgumentException("Category must be Artist, Group, Character, or Series");
        }
        HashSet<char> firstChars = [];
        foreach (var tag in tags) {
            firstChars.Add(tag.Value[0]);
        }
        foreach (char c in firstChars) {
            string url = string.Format(
                NON_MFT_TAG_DATA_HTML,
                OriginalGalleryInfoDTO.CATEGORY_PROP_KEY_DICT[category],
                c >= 'a' && c <= 'z' ? c : "123"
            );
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string html = await response.Content.ReadAsStringAsync();
                Match contentMatch = HtmlContentRegex().Match(html);
                if (contentMatch.Success) {
                    string content = contentMatch.Groups[1].Value;
                    MatchCollection tagInfoMatches = TagInfoRegex().Matches(content);
                    using HitomiContext dbContext = dbContextFactory.CreateDbContext();
                    foreach (Match match in tagInfoMatches) {
                        string tagValue = match.Groups[1].Value;
                        int galleryCount = int.Parse(match.Groups[2].Value);
                        Tag? existingTag = dbContext.Tags.FirstOrDefault(t => t.Value == tagValue && t.Category == category);
                        if (existingTag == null) {
                            dbContext.Tags.Add(new Tag() {
                                Category = category,
                                Value = tagValue,
                                GalleryCount = galleryCount
                            });
                        } else {
                            existingTag.GalleryCount = galleryCount;
                        }
                    }
                    dbContext.SaveChanges();
                }
            }
        }
    }
        
    public async Task FetchUpdateMFTTags(IEnumerable<TagDTO> tags) {
        HashSet<char> firstChars = [];
        foreach (var tag in tags) {
            firstChars.Add(tag.Value[0]);
        }
        foreach (char c in firstChars) {
            string url = string.Format(MFT_TAG_DATA_HTML, c >= 'a' && c <= 'z' ? c : "123");
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string html = await response.Content.ReadAsStringAsync();
                Match contentMatch = HtmlContentRegex().Match(html);
                if (contentMatch.Success) {
                    string content = contentMatch.Groups[1].Value;
                    MatchCollection tagInfoMatches = TagInfoRegex().Matches(content);
                    using HitomiContext dbContext = dbContextFactory.CreateDbContext();
                    foreach (Match match in tagInfoMatches) {
                        string tagValueWithSymbol = match.Groups[1].Value;
                        string tagValue;
                        TagCategory category;
                        if (tagValueWithSymbol.EndsWith(MALE_SYMBOL)) {
                            tagValue = tagValueWithSymbol[..^2];
                            category = TagCategory.Male;
                        } else if (tagValueWithSymbol.EndsWith(FEMALE_SYMBOL)) {
                            tagValue = tagValueWithSymbol[..^2];
                            category = TagCategory.Female;
                        } else {
                            tagValue = tagValueWithSymbol;
                            category = TagCategory.Tag;
                        }
                        int galleryCount = int.Parse(match.Groups[2].Value);
                        Tag? existingTag = dbContext.Tags.FirstOrDefault(t => t.Value == tagValue && t.Category == category);
                        if (existingTag == null) {
                            dbContext.Tags.Add(new Tag() {
                                Category = category,
                                Value = tagValue,
                                GalleryCount = galleryCount
                            });
                        } else {
                            existingTag.GalleryCount = galleryCount;
                        }
                    }
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
