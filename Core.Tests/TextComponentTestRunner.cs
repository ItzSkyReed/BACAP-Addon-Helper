using Core.SNBT;
using Core.TextComponents;
using Core.TextComponents.Components;
using Core.TextComponents.Events;
using Xunit.Abstractions;

namespace Core.Tests;

public class TextComponentTests(ITestOutputHelper output)
{

    [Fact]
    public void TestTextComponentParsingAndRoundtrip()
    {
        // 2. Пишем сложную ванильную SNBT-строку
        string snbtInput = """
        {
            text: "Привет, ",
            color: "gold",
            bold: true,
            click_event: { action: "run_command", command: "help" },
            extra: [
                { text: "мир!", color: "aqua", italic: true },
                " А это шорткат-текст строкой!"
            ]
        }
        """;

        output.WriteLine("=== ИСХОДНЫЙ SNBT ===");
        output.WriteLine(snbtInput);
        output.WriteLine("");

        try
        {
            // Шаг 2. Парсим сырую строку в AST-узел
            var snbtNode = SnbtParser.Parse(snbtInput);

            // Шаг 3. Превращаем AST-узел в наши классы
            var component = TextComponentParser.Parse(snbtNode);

            // Шаг 4. Выводим результаты десериализации через _output
            output.WriteLine("=== РЕЗУЛЬТАТ ПАРСИНГА ===");
            output.WriteLine($"Корневой тип: {component.GetType().Name}");

            if (component is PlainTextComponent plain)
            {
                output.WriteLine($"Текст: \"{plain.Text}\"");
            }

            if (component.Style != null)
            {
                output.WriteLine($"Стиль -> Цвет: {component.Style.Color}, Жирный: {component.Style.Bold}");

                if (component.Style.ClickEvent is RunCommandClickEvent cmdEvent)
                {
                    output.WriteLine($"Клик-ивент -> Команда: /{cmdEvent.Command}");
                }
            }

            if (component.Extra is { Count: > 0 })
            {
                output.WriteLine($"Элементы Extra ({component.Extra.Count}):");
                foreach (var extra in component.Extra)
                {
                    string extraText = extra is PlainTextComponent p ? p.Text : "сложный компонент";
                    output.WriteLine($"  - [{extra.GetType().Name}] \"{extraText}\" (Цвет: {extra.Style?.Color ?? "наследуется"})");
                }
            }

            // Шаг 5. Сериализуем обратно в SNBT
            var reserializedNode = component.ToSnbt();
            string outputSnbt = reserializedNode.ToSnbtString(pretty: true);

            output.WriteLine("");
            output.WriteLine("=== СЕРИАЛИЗАЦИЯ ОБРАТНО В SNBT ===");
            output.WriteLine(outputSnbt);
        }
        catch (Exception ex)
        {
            output.WriteLine($"❌ Ошибка: {ex.Message}");
            output.WriteLine(ex.StackTrace ?? string.Empty);
            Assert.Fail(ex.Message); // Валим тест при исключении
        }
    }
}