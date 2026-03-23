using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Group4337
{
    public partial class Valeeva_4337 : Window
    {
        public Valeeva_4337()
        {
            InitializeComponent();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orders = new List<Order>();

                using (var package = new ExcelPackage(new FileInfo("2.xlsx")))
                {
                    var sheet = package.Workbook.Worksheets[0];

                    for (int i = 2; i <= sheet.Dimension.Rows; i++)
                    {
                        orders.Add(new Order
                        {
                            OrderCode = sheet.Cells[i, 2].Text,
                            CreationDate = DateTime.Parse(sheet.Cells[i, 3].Text),
                            ClientCode = sheet.Cells[i, 4].Text,
                            Services = sheet.Cells[i, 5].Text
                        });
                    }
                }

                using (var db = new AppDbContext())
                {
                    db.Orders.RemoveRange(db.Orders);
                    db.Orders.AddRange(orders);
                    db.SaveChanges();
                }

                MessageBox.Show("Импорт завершен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += "\n\n" + ex.InnerException.Message;

                MessageBox.Show($"Ошибка импорта:\n{msg}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Order> data;

                using (var db = new AppDbContext())
                {
                    data = db.Orders.ToList();
                }

                if (!data.Any())
                {
                    MessageBox.Show("Нет данных для экспорта", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var grouped = data.GroupBy(x => x.GetRentalCategory());

                using (var package = new ExcelPackage())
                {
                    foreach (var group in grouped)
                    {
                        var sheet = package.Workbook.Worksheets.Add(group.Key);

                        sheet.Cells[1, 1].Value = "Id";
                        sheet.Cells[1, 2].Value = "Код заказа";
                        sheet.Cells[1, 3].Value = "Дата создания";
                        sheet.Cells[1, 4].Value = "Код клиента";
                        sheet.Cells[1, 5].Value = "Услуги";

                        int row = 2;

                        foreach (var item in group)
                        {
                            sheet.Cells[row, 1].Value = item.Id;
                            sheet.Cells[row, 2].Value = item.OrderCode;
                            sheet.Cells[row, 3].Value = item.CreationDate.ToString("dd.MM.yyyy");
                            sheet.Cells[row, 4].Value = item.ClientCode;
                            sheet.Cells[row, 5].Value = item.Services;
                            row++;
                        }

                        sheet.Cells.AutoFitColumns();
                    }

                    string fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    File.WriteAllBytes(fileName, package.GetAsByteArray());

                    MessageBox.Show($"Экспорт завершен!\nФайл: {fileName}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                    msg += "\n\n" + ex.InnerException.Message;

                MessageBox.Show($"Ошибка экспорта:\n{msg}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ImportJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var json = File.ReadAllText("2.json");
                var orders = JsonSerializer.Deserialize<List<Order>>(json);

                using (var db = new AppDbContext())
                {
                    db.Orders.RemoveRange(db.Orders);
                    db.Orders.AddRange(orders);
                    db.SaveChanges();
                }

                MessageBox.Show("JSON импортирован!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Order> data;

                using (var db = new AppDbContext())
                {
                    data = db.Orders.ToList();
                }

                if (!data.Any())
                {
                    MessageBox.Show("Нет данных");
                    return;
                }

                var grouped = data.GroupBy(x => x.GetRentalCategory());

                string fileName = $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.docx";

                using (WordprocessingDocument doc =
                    WordprocessingDocument.Create(fileName, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = new Body();

                    foreach (var group in grouped)
                    {
                        body.Append(new Paragraph(new Run(new Text(group.Key))));

                        foreach (var item in group)
                        {
                            body.Append(new Paragraph(new Run(new Text(
                                $"Id: {item.Id}, Код: {item.OrderCode}, Дата: {item.CreationDate:dd.MM.yyyy}, Клиент: {item.ClientCode}, Услуги: {item.Services}"
                            ))));
                        }

                        body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                    }

                    var firstDate = data.Min(x => x.CreationDate);
                    var lastDate = data.Max(x => x.CreationDate);

                    body.Append(new Paragraph(new Run(new Text(
                        $"Первый заказ: {firstDate:dd.MM.yyyy}"
                    ))));

                    body.Append(new Paragraph(new Run(new Text(
                        $"Последний заказ: {lastDate:dd.MM.yyyy}"
                    ))));

                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                }

                MessageBox.Show($"Word создан: {fileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}