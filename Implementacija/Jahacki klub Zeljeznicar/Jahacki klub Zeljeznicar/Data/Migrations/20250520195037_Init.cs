using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jahacki_klub_Zeljeznicar.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Konj",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Spol = table.Column<int>(type: "int", nullable: false),
                    Boja = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konj", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Kategorija = table.Column<int>(type: "int", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nivo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clanarina",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PocetakClanarine = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IstekClanarine = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clanarina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clanarina_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RezervatorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trail_User_RezervatorId",
                        column: x => x.RezervatorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trening",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nivo = table.Column<int>(type: "int", nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxBrClanova = table.Column<int>(type: "int", nullable: false),
                    TrenerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trening", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trening_User_TrenerId",
                        column: x => x.TrenerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trail_Konj",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrailId = table.Column<int>(type: "int", nullable: false),
                    KonjId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trail_Konj", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trail_Konj_Konj_KonjId",
                        column: x => x.KonjId,
                        principalTable: "Konj",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trail_Konj_Trail_TrailId",
                        column: x => x.TrailId,
                        principalTable: "Trail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trening_Konj",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreningId = table.Column<int>(type: "int", nullable: false),
                    KonjId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trening_Konj", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trening_Konj_Konj_KonjId",
                        column: x => x.KonjId,
                        principalTable: "Konj",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trening_Konj_Trening_TreningId",
                        column: x => x.TreningId,
                        principalTable: "Trening",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trening_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreningId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trening_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trening_User_Trening_TreningId",
                        column: x => x.TreningId,
                        principalTable: "Trening",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trening_User_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clanarina_UserId",
                table: "Clanarina",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trail_RezervatorId",
                table: "Trail",
                column: "RezervatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Trail_Konj_KonjId",
                table: "Trail_Konj",
                column: "KonjId");

            migrationBuilder.CreateIndex(
                name: "IX_Trail_Konj_TrailId",
                table: "Trail_Konj",
                column: "TrailId");

            migrationBuilder.CreateIndex(
                name: "IX_Trening_TrenerId",
                table: "Trening",
                column: "TrenerId");

            migrationBuilder.CreateIndex(
                name: "IX_Trening_Konj_KonjId",
                table: "Trening_Konj",
                column: "KonjId");

            migrationBuilder.CreateIndex(
                name: "IX_Trening_Konj_TreningId",
                table: "Trening_Konj",
                column: "TreningId");

            migrationBuilder.CreateIndex(
                name: "IX_Trening_User_TreningId",
                table: "Trening_User",
                column: "TreningId");

            migrationBuilder.CreateIndex(
                name: "IX_Trening_User_UserId",
                table: "Trening_User",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clanarina");

            migrationBuilder.DropTable(
                name: "Trail_Konj");

            migrationBuilder.DropTable(
                name: "Trening_Konj");

            migrationBuilder.DropTable(
                name: "Trening_User");

            migrationBuilder.DropTable(
                name: "Trail");

            migrationBuilder.DropTable(
                name: "Konj");

            migrationBuilder.DropTable(
                name: "Trening");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
