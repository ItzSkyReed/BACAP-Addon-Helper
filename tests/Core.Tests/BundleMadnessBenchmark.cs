using System.Diagnostics;
using Core.DataComponents;
using Core.DataComponents.Components;
using Core.Items;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Xunit.Abstractions;

namespace Core.Tests;

public class BundleMadnessBenchmark(ITestOutputHelper output)
{
    [Fact]
    public void Run()
    {
        int depth = 250;

        // 1. Умная регистрация всех компонентов проекта за один проход с помощью Reflection
        ComponentRegistry.RegisterAll();

        output.WriteLine($"=== ЗАПУСК МАТРЁШКИ ИЗ МЕШКОВ (Глубина: {depth}) ===");

        // 2. Создаем дно матрёшки — заветную иглу
        var innermostItem = new ItemStack("minecraft:stick");
        innermostItem.Components.Set(new DamageComponent(9999));

        var currentItem = innermostItem;

        // Оборачиваем мешок в мешок depth раз
        for (int i = 1; i <= depth; i++)
        {
            var bundle = new ItemStack("minecraft:bundle");
            bundle.Components.Set(new BundleContentsComponent(new List<ItemStack> {currentItem} ));
            currentItem = bundle;
        }

        var rootBundle = currentItem;

        // 3. Сериализация в чистый SNBT
        var swSerialize = Stopwatch.StartNew();

        // Получаем корневой SNBT компаунд компонентов
        var bundleComponent = rootBundle.Components.Get<BundleContentsComponent>()!;
        var snbtNode = bundleComponent.ToSnbt();
        string snbtString = snbtNode.ToSnbtString(pretty: false);

        swSerialize.Stop();

        output.WriteLine($"✅ Сериализация завершена за: {swSerialize.Elapsed.TotalMilliseconds:F3} мс");
        output.WriteLine($"📏 Длина итоговой SNBT-строки: {snbtString.Length:N0} символов (~{snbtString.Length / 1024.0:F2} KB)");
        output.WriteLine($"👀 Превью начала: {snbtString[..Math.Min(120, snbtString.Length)]}...");
        output.WriteLine("");

        // 4. Замер памяти и парсинга
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memBefore = GC.GetAllocatedBytesForCurrentThread();
        var swParse = Stopwatch.StartNew();

        // Шаг А: Парсинг строки в AST дерево через Pidgin
        ISnbtNode parsedAst = SnbtParser.Parse(snbtString);

        // Шаг Б: Десериализация AST в C# объекты.
        // Кастуем явно к (SnbtList), так как новый интерфейс строго типизирован!
        var parsedBundle = BundleContentsComponent.Parse((SnbtList)parsedAst);

        swParse.Stop();
        long memAllocated = GC.GetAllocatedBytesForCurrentThread() - memBefore;

        output.WriteLine("=== ⚡ РЕЗУЛЬТАТЫ ПАРСИНГА И АЛЛОКАЦИЙ ===");
        output.WriteLine($"⏱ Время полного парсинга (SNBT -> AST -> C# классы): {swParse.Elapsed.TotalMilliseconds:F3} мс");
        output.WriteLine($"💾 Аллоцировано памяти в куче: {memAllocated / 1024.0:F2} KB ({memAllocated:N0} байт)");

        // 5. Рекурсивный спуск: считаем реальные объекты в памяти
        int totalBundlesCount = 0;
        int totalItemStacksCount = 0;
        ItemStack? leaf;

        var cur = parsedBundle;
        while (true)
        {
            totalBundlesCount++;
            totalItemStacksCount += cur.Items.Count;

            var nextItem = cur.Items[0];
            if (nextItem.Components.TryGet<BundleContentsComponent>(out var nextBundle))
            {
                cur = nextBundle;
            }
            else
            {
                leaf = nextItem;
                break;
            }
        }

        output.WriteLine("=== 🔍 ПРОВЕРКА ДЕРЕВА ОБЪЕКТОВ ===");
        output.WriteLine($"📦 Распарсено вложенных BundleContentsComponent: {totalBundlesCount}");
        output.WriteLine($"🗡 Всего создано экземпляров ItemStack: {totalItemStacksCount}");

        {
            var damage = leaf.Components.TryGet<DamageComponent>(out var dmg) ? dmg.Damage : 0;
            output.WriteLine($"🎯 На самом дне найден: '{leaf.Id}' с уроном {damage}");
        }
    }
}