using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using EbayClone.API.DTOs.Audit;

namespace EbayClone.API.Services;

public static class AuditLogExcelExporter
{
    public static byte[] Create(IEnumerable<AuditLogDto> logs)
    {
        var rows = logs.Select(log => new[]
        {
            log.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"),
            log.ActorId?.ToString() ?? string.Empty,
            log.Action,
            log.Resource,
            log.ResourceId?.ToString() ?? string.Empty,
            log.Metadata ?? string.Empty
        }).ToList();
        rows.Insert(0, ["Created at (UTC)", "Actor ID", "Action", "Resource", "Resource ID", "Metadata"]);

        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                </Types>
                """);
            AddEntry(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Audit Logs" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            AddEntry(archive, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);
            AddEntry(archive, "xl/styles.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                  <fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts>
                  <fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>
                  <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
                  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
                  <cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/><xf numFmtId="0" fontId="1" fillId="0" borderId="0" applyFont="1"/></cellXfs>
                </styleSheet>
                """);
            AddEntry(archive, "xl/worksheets/sheet1.xml", BuildSheet(rows));
        }

        return output.ToArray();
    }

    private static string BuildSheet(IReadOnlyList<string[]> rows)
    {
        var sheet = new XElement(XName.Get("worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
            new XElement(XName.Get("sheetData", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
                rows.Select((row, rowIndex) => new XElement(XName.Get("row", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
                    new XAttribute("r", rowIndex + 1),
                    row.Select((value, columnIndex) =>
                    {
                        var cell = new XElement(XName.Get("c", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
                            new XAttribute("r", $"{(char)('A' + columnIndex)}{rowIndex + 1}"),
                            new XAttribute("t", "inlineStr"),
                            new XElement(XName.Get("is", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"),
                                new XElement(XName.Get("t", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"), value)));
                        if (rowIndex == 0) cell.SetAttributeValue("s", "1");
                        return cell;
                    })))));
        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), sheet).ToString(SaveOptions.DisableFormatting);
    }

    private static void AddEntry(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
