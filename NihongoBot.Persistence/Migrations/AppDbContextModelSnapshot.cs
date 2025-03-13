﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NihongoBot.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NihongoBot.Domain.Aggregates.Hiragana.Kana", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Character")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("character varying(2)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Romaji")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Character")
                        .IsUnique();

                    b.ToTable("Kanas");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Character = "あ",
                            Romaji = "a",
                            Type = 0
                        },
                        new
                        {
                            Id = 2,
                            Character = "い",
                            Romaji = "i",
                            Type = 0
                        },
                        new
                        {
                            Id = 3,
                            Character = "う",
                            Romaji = "u",
                            Type = 0
                        },
                        new
                        {
                            Id = 4,
                            Character = "え",
                            Romaji = "e",
                            Type = 0
                        },
                        new
                        {
                            Id = 5,
                            Character = "お",
                            Romaji = "o",
                            Type = 0
                        },
                        new
                        {
                            Id = 6,
                            Character = "か",
                            Romaji = "ka",
                            Type = 0
                        },
                        new
                        {
                            Id = 7,
                            Character = "き",
                            Romaji = "ki",
                            Type = 0
                        },
                        new
                        {
                            Id = 8,
                            Character = "く",
                            Romaji = "ku",
                            Type = 0
                        },
                        new
                        {
                            Id = 9,
                            Character = "け",
                            Romaji = "ke",
                            Type = 0
                        },
                        new
                        {
                            Id = 10,
                            Character = "こ",
                            Romaji = "ko",
                            Type = 0
                        },
                        new
                        {
                            Id = 11,
                            Character = "さ",
                            Romaji = "sa",
                            Type = 0
                        },
                        new
                        {
                            Id = 12,
                            Character = "し",
                            Romaji = "shi",
                            Type = 0
                        },
                        new
                        {
                            Id = 13,
                            Character = "す",
                            Romaji = "su",
                            Type = 0
                        },
                        new
                        {
                            Id = 14,
                            Character = "せ",
                            Romaji = "se",
                            Type = 0
                        },
                        new
                        {
                            Id = 15,
                            Character = "そ",
                            Romaji = "so",
                            Type = 0
                        },
                        new
                        {
                            Id = 16,
                            Character = "た",
                            Romaji = "ta",
                            Type = 0
                        },
                        new
                        {
                            Id = 17,
                            Character = "ち",
                            Romaji = "chi",
                            Type = 0
                        },
                        new
                        {
                            Id = 18,
                            Character = "つ",
                            Romaji = "tsu",
                            Type = 0
                        },
                        new
                        {
                            Id = 19,
                            Character = "て",
                            Romaji = "te",
                            Type = 0
                        },
                        new
                        {
                            Id = 20,
                            Character = "と",
                            Romaji = "to",
                            Type = 0
                        },
                        new
                        {
                            Id = 21,
                            Character = "な",
                            Romaji = "na",
                            Type = 0
                        },
                        new
                        {
                            Id = 22,
                            Character = "に",
                            Romaji = "ni",
                            Type = 0
                        },
                        new
                        {
                            Id = 23,
                            Character = "ぬ",
                            Romaji = "nu",
                            Type = 0
                        },
                        new
                        {
                            Id = 24,
                            Character = "ね",
                            Romaji = "ne",
                            Type = 0
                        },
                        new
                        {
                            Id = 25,
                            Character = "の",
                            Romaji = "no",
                            Type = 0
                        },
                        new
                        {
                            Id = 26,
                            Character = "は",
                            Romaji = "ha",
                            Type = 0
                        },
                        new
                        {
                            Id = 27,
                            Character = "ひ",
                            Romaji = "hi",
                            Type = 0
                        },
                        new
                        {
                            Id = 28,
                            Character = "ふ",
                            Romaji = "fu",
                            Type = 0
                        },
                        new
                        {
                            Id = 29,
                            Character = "へ",
                            Romaji = "he",
                            Type = 0
                        },
                        new
                        {
                            Id = 30,
                            Character = "ほ",
                            Romaji = "ho",
                            Type = 0
                        },
                        new
                        {
                            Id = 31,
                            Character = "ま",
                            Romaji = "ma",
                            Type = 0
                        },
                        new
                        {
                            Id = 32,
                            Character = "み",
                            Romaji = "mi",
                            Type = 0
                        },
                        new
                        {
                            Id = 33,
                            Character = "む",
                            Romaji = "mu",
                            Type = 0
                        },
                        new
                        {
                            Id = 34,
                            Character = "め",
                            Romaji = "me",
                            Type = 0
                        },
                        new
                        {
                            Id = 35,
                            Character = "も",
                            Romaji = "mo",
                            Type = 0
                        },
                        new
                        {
                            Id = 36,
                            Character = "や",
                            Romaji = "ya",
                            Type = 0
                        },
                        new
                        {
                            Id = 37,
                            Character = "ゆ",
                            Romaji = "yu",
                            Type = 0
                        },
                        new
                        {
                            Id = 38,
                            Character = "よ",
                            Romaji = "yo",
                            Type = 0
                        },
                        new
                        {
                            Id = 39,
                            Character = "ら",
                            Romaji = "ra",
                            Type = 0
                        },
                        new
                        {
                            Id = 40,
                            Character = "り",
                            Romaji = "ri",
                            Type = 0
                        },
                        new
                        {
                            Id = 41,
                            Character = "る",
                            Romaji = "ru",
                            Type = 0
                        },
                        new
                        {
                            Id = 42,
                            Character = "れ",
                            Romaji = "re",
                            Type = 0
                        },
                        new
                        {
                            Id = 43,
                            Character = "ろ",
                            Romaji = "ro",
                            Type = 0
                        },
                        new
                        {
                            Id = 44,
                            Character = "わ",
                            Romaji = "wa",
                            Type = 0
                        },
                        new
                        {
                            Id = 45,
                            Character = "を",
                            Romaji = "wo",
                            Type = 0
                        },
                        new
                        {
                            Id = 46,
                            Character = "ん",
                            Romaji = "n",
                            Type = 0
                        },
                        new
                        {
                            Id = 47,
                            Character = "きゃ",
                            Romaji = "kya",
                            Type = 0
                        },
                        new
                        {
                            Id = 48,
                            Character = "きゅ",
                            Romaji = "kyu",
                            Type = 0
                        },
                        new
                        {
                            Id = 49,
                            Character = "きょ",
                            Romaji = "kyo",
                            Type = 0
                        },
                        new
                        {
                            Id = 50,
                            Character = "しゃ",
                            Romaji = "sha",
                            Type = 0
                        },
                        new
                        {
                            Id = 51,
                            Character = "しゅ",
                            Romaji = "shu",
                            Type = 0
                        },
                        new
                        {
                            Id = 52,
                            Character = "しょ",
                            Romaji = "sho",
                            Type = 0
                        },
                        new
                        {
                            Id = 53,
                            Character = "ちゃ",
                            Romaji = "cha",
                            Type = 0
                        },
                        new
                        {
                            Id = 54,
                            Character = "ちゅ",
                            Romaji = "chu",
                            Type = 0
                        },
                        new
                        {
                            Id = 55,
                            Character = "ちょ",
                            Romaji = "cho",
                            Type = 0
                        },
                        new
                        {
                            Id = 56,
                            Character = "にゃ",
                            Romaji = "nya",
                            Type = 0
                        },
                        new
                        {
                            Id = 57,
                            Character = "にゅ",
                            Romaji = "nyu",
                            Type = 0
                        },
                        new
                        {
                            Id = 58,
                            Character = "にょ",
                            Romaji = "nyo",
                            Type = 0
                        },
                        new
                        {
                            Id = 59,
                            Character = "ひゃ",
                            Romaji = "hya",
                            Type = 0
                        },
                        new
                        {
                            Id = 60,
                            Character = "ひゅ",
                            Romaji = "hyu",
                            Type = 0
                        },
                        new
                        {
                            Id = 61,
                            Character = "ひょ",
                            Romaji = "hyo",
                            Type = 0
                        },
                        new
                        {
                            Id = 62,
                            Character = "みゃ",
                            Romaji = "mya",
                            Type = 0
                        },
                        new
                        {
                            Id = 63,
                            Character = "みゅ",
                            Romaji = "myu",
                            Type = 0
                        },
                        new
                        {
                            Id = 64,
                            Character = "みょ",
                            Romaji = "myo",
                            Type = 0
                        },
                        new
                        {
                            Id = 65,
                            Character = "りゃ",
                            Romaji = "rya",
                            Type = 0
                        },
                        new
                        {
                            Id = 66,
                            Character = "りゅ",
                            Romaji = "ryu",
                            Type = 0
                        },
                        new
                        {
                            Id = 67,
                            Character = "りょ",
                            Romaji = "ryo",
                            Type = 0
                        });
                });

            modelBuilder.Entity("NihongoBot.Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Streak")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<long>("TelegramId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TelegramId")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("NihongoBot.Domain.Aggregates.Hiragana.Kana", b =>
                {
                    b.OwnsMany("NihongoBot.Domain.Aggregates.Hiragana.KanaVariant", "Variants", b1 =>
                        {
                            b1.Property<string>("Character")
                                .HasMaxLength(2)
                                .HasColumnType("character varying(2)");

                            b1.Property<int>("KanaId")
                                .HasColumnType("integer");

                            b1.Property<string>("Romaji")
                                .IsRequired()
                                .HasMaxLength(5)
                                .HasColumnType("character varying(5)");

                            b1.HasKey("Character");

                            b1.HasIndex("KanaId");

                            b1.ToTable("KanaVariant");

                            b1.WithOwner()
                                .HasForeignKey("KanaId");

                            b1.HasData(
                                new
                                {
                                    Character = "ゔ",
                                    KanaId = 3,
                                    Romaji = "vu"
                                },
                                new
                                {
                                    Character = "が",
                                    KanaId = 6,
                                    Romaji = "ga"
                                },
                                new
                                {
                                    Character = "ぎ",
                                    KanaId = 7,
                                    Romaji = "gi"
                                },
                                new
                                {
                                    Character = "ぐ",
                                    KanaId = 8,
                                    Romaji = "gu"
                                },
                                new
                                {
                                    Character = "げ",
                                    KanaId = 9,
                                    Romaji = "ge"
                                },
                                new
                                {
                                    Character = "ご",
                                    KanaId = 10,
                                    Romaji = "go"
                                },
                                new
                                {
                                    Character = "ざ",
                                    KanaId = 11,
                                    Romaji = "za"
                                },
                                new
                                {
                                    Character = "じ",
                                    KanaId = 12,
                                    Romaji = "ji"
                                },
                                new
                                {
                                    Character = "ず",
                                    KanaId = 13,
                                    Romaji = "zu"
                                },
                                new
                                {
                                    Character = "ぜ",
                                    KanaId = 14,
                                    Romaji = "ze"
                                },
                                new
                                {
                                    Character = "ぞ",
                                    KanaId = 15,
                                    Romaji = "zo"
                                },
                                new
                                {
                                    Character = "だ",
                                    KanaId = 16,
                                    Romaji = "da"
                                },
                                new
                                {
                                    Character = "ぢ",
                                    KanaId = 17,
                                    Romaji = "ji"
                                },
                                new
                                {
                                    Character = "づ",
                                    KanaId = 18,
                                    Romaji = "zu"
                                },
                                new
                                {
                                    Character = "で",
                                    KanaId = 19,
                                    Romaji = "de"
                                },
                                new
                                {
                                    Character = "ど",
                                    KanaId = 20,
                                    Romaji = "do"
                                },
                                new
                                {
                                    Character = "ば",
                                    KanaId = 26,
                                    Romaji = "ba"
                                },
                                new
                                {
                                    Character = "ぱ",
                                    KanaId = 26,
                                    Romaji = "pa"
                                },
                                new
                                {
                                    Character = "び",
                                    KanaId = 27,
                                    Romaji = "bi"
                                },
                                new
                                {
                                    Character = "ぴ",
                                    KanaId = 27,
                                    Romaji = "pi"
                                },
                                new
                                {
                                    Character = "ぶ",
                                    KanaId = 28,
                                    Romaji = "bu"
                                },
                                new
                                {
                                    Character = "ぷ",
                                    KanaId = 28,
                                    Romaji = "pu"
                                },
                                new
                                {
                                    Character = "べ",
                                    KanaId = 29,
                                    Romaji = "be"
                                },
                                new
                                {
                                    Character = "ぺ",
                                    KanaId = 29,
                                    Romaji = "pe"
                                },
                                new
                                {
                                    Character = "ぼ",
                                    KanaId = 30,
                                    Romaji = "bo"
                                },
                                new
                                {
                                    Character = "ぽ",
                                    KanaId = 30,
                                    Romaji = "po"
                                });
                        });

                    b.Navigation("Variants");
                });
#pragma warning restore 612, 618
        }
    }
}
