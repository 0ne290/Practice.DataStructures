using System.Globalization;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections;

public sealed class DynamicArrayLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void DynamicArray_RandomData_DemonstratesGrowthAndIndexedAccess()
    {
        // Arrange
        const string seedEnvironmentVariable = "DATASTRUCTURES_LAB_SEED";
        const int initialCapacity = 2;
        const int itemCount = 24;
        const int replacementCount = 5;
        const int removalCount = 5;
        const int minimumValue = -9;
        const int maximumValue = 9;

        string? suppliedSeed = Environment.GetEnvironmentVariable(seedEnvironmentVariable);
        int seed;
        if (suppliedSeed is null)
        {
            seed = Random.Shared.Next();
        }
        else if (!int.TryParse(suppliedSeed, NumberStyles.Integer, CultureInfo.InvariantCulture, out seed))
        {
            throw new InvalidOperationException($"{seedEnvironmentVariable} must contain a 32-bit integer seed.");
        }

        string seedSource = suppliedSeed is null ? "новый случайный" : seedEnvironmentVariable;
        output.WriteLine("DynamicArray<int>: добавление, рост, перебор, чтение, замена и удаление");
        output.WriteLine($"Seed={seed} ({seedSource}); начальная вместимость={initialCapacity}; элементов={itemCount}; замен={replacementCount}; случайных удалений={removalCount}");
        output.WriteLine($"Повторить этот прогон: --environment {seedEnvironmentVariable}={seed}");
        output.WriteLine($"Диапазон значений: [{minimumValue}, {maximumValue}]; значения для замены: [100, 199]");

        var random = new Random(seed);
        var array = new DynamicArray<int>(initialCapacity);
        int[] expected = new int[itemCount];
        for (int index = 0; index < expected.Length; index++)
        {
            expected[index] = random.Next(minimumValue, maximumValue + 1);
        }

        output.WriteLine($"Исходные данные: [{string.Join(", ", expected)}]");
        output.WriteLine($"Пустой массив: Count={array.Count}, Capacity={array.Capacity}");
        Assert.Equal(0, array.Count);
        Assert.Equal(initialCapacity, array.Capacity);
        Assert.Empty(array);

        // Act: добавление с наблюдением за вместимостью.
        output.WriteLine("");
        output.WriteLine("1. Добавляем элементы по одному");
        int capacityChanges = 0;
        for (int index = 0; index < expected.Length; index++)
        {
            int previousCapacity = array.Capacity;
            array.Add(expected[index]);

            string growth = "";
            if (array.Capacity != previousCapacity)
            {
                capacityChanges++;
                growth = $"  рост: {previousCapacity} -> {array.Capacity}";
            }

            output.WriteLine($"{index + 1,2}. Add({expected[index],3})  Count={array.Count,2}  Capacity={array.Capacity,2}{growth}");
            Assert.Equal(index + 1, array.Count);
            Assert.True(array.Capacity >= array.Count);
        }

        // Assert: добавленные значения сохранили порядок.
        Assert.Equal(expected, Enumerable.Range(0, array.Count).Select(index => array[index]));
        output.WriteLine($"Изменений вместимости: {capacityChanges}; свободных мест: {array.Capacity - array.Count}");
        output.WriteLine("Порядок и значения всех элементов проверены.");

        // Act / Assert: полный обход через IEnumerable<T>.
        output.WriteLine("");
        output.WriteLine("2. Перебираем список через foreach");
        var enumerated = new List<int>();
        foreach (int item in array)
        {
            enumerated.Add(item);
        }

        Assert.Equal(expected, enumerated);
        output.WriteLine($"foreach: [{string.Join(", ", enumerated)}]");
        output.WriteLine($"Посещено {enumerated.Count} элементов при Count={array.Count}, Capacity={array.Capacity}; резерв в обход не попал.");

        // Act / Assert: чтение по случайному допустимому индексу.
        output.WriteLine("");
        output.WriteLine("3. Читаем элемент по индексу");
        int sampleIndex = random.Next(array.Count);
        int sampleValue = array[sampleIndex];
        Assert.Equal(expected[sampleIndex], sampleValue);
        output.WriteLine($"array[{sampleIndex}] = {sampleValue}");

        // Act: замены по случайным индексам.
        output.WriteLine("");
        output.WriteLine("4. Заменяем элементы через индексатор");
        int capacityBeforeReplacement = array.Capacity;
        for (int replacement = 0; replacement < replacementCount; replacement++)
        {
            int index = random.Next(array.Count);
            int previousValue = array[index];
            int nextValue = random.Next(100, 200);
            array[index] = nextValue;
            expected[index] = nextValue;
            output.WriteLine($"array[{index,2}]: {previousValue,3} -> {nextValue,3}  Count={array.Count}, Capacity={array.Capacity}");
        }

        // Assert: замены сохранили размер и вместимость.
        Assert.Equal(itemCount, array.Count);
        Assert.Equal(capacityBeforeReplacement, array.Capacity);
        int[] actual = [.. array];
        Assert.Equal(expected, actual);
        output.WriteLine($"Итоговое содержимое: [{string.Join(", ", actual)}]");
        output.WriteLine("Содержимое сверено с эталонным массивом; Count и Capacity сохранились.");

        // Act / Assert: изменения инвалидируют уже полученные перечислители.
        output.WriteLine("");
        output.WriteLine("5. Меняем список во время обхода");
        using (IEnumerator<int> iterator = array.GetEnumerator())
        {
            Assert.True(iterator.MoveNext());
            int addedItem = random.Next(minimumValue, maximumValue + 1);
            array.Add(addedItem);
            expected = [.. expected, addedItem];
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                iterator.MoveNext();
            });
            output.WriteLine($"Add({addedItem}) выполнен; продолжение старого обхода: {exception.GetType().Name}");
        }

        using (IEnumerator<int> iterator = array.GetEnumerator())
        {
            Assert.True(iterator.MoveNext());
            int index = random.Next(array.Count);
            int replacement = random.Next(100, 200);
            array[index] = replacement;
            expected[index] = replacement;
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                iterator.MoveNext();
            });
            output.WriteLine($"array[{index}] = {replacement} выполнено; продолжение старого обхода: {exception.GetType().Name}");
        }

        int[] freshPass = [.. array];
        Assert.Equal(expected, freshPass);
        output.WriteLine($"Новый обход работает: [{string.Join(", ", freshPass)}]");

        // Act / Assert: удаление по случайным индексам с сохранением порядка.
        output.WriteLine("");
        output.WriteLine("6. Удаляем элементы по случайным индексам");
        var removalReference = new List<int>(expected);
        int capacityBeforeRemoval = array.Capacity;
        for (int removal = 0; removal < removalCount; removal++)
        {
            int index = random.Next(array.Count);
            int removedValue = array[index];
            using IEnumerator<int> iterator = array.GetEnumerator();
            Assert.True(iterator.MoveNext());

            array.RemoveAt(index);
            removalReference.RemoveAt(index);

            Assert.Equal(removalReference.Count, array.Count);
            Assert.Equal(capacityBeforeRemoval, array.Capacity);
            Assert.Equal(removalReference, array);
            Assert.Throws<InvalidOperationException>(() =>
            {
                iterator.MoveNext();
            });
            output.WriteLine($"RemoveAt({index}): удалено {removedValue}; Count={array.Count}, Capacity={array.Capacity}; старый обход инвалидирован.");
            output.WriteLine($"Осталось: [{string.Join(", ", array)}]");
        }

        // Act / Assert: удаление всех элементов не меняет вместимость.
        output.WriteLine("");
        output.WriteLine("7. Удаляем оставшиеся элементы с конца и используем массив повторно");
        while (removalReference.Count > 0)
        {
            int lastIndex = removalReference.Count - 1;
            array.RemoveAt(lastIndex);
            removalReference.RemoveAt(lastIndex);

            Assert.Equal(removalReference.Count, array.Count);
            Assert.Equal(capacityBeforeRemoval, array.Capacity);
            Assert.Equal(removalReference, array);
        }

        Assert.Empty(array);
        output.WriteLine($"Все элементы удалены: Count={array.Count}, Capacity={array.Capacity}");
        int reusedValue = random.Next(minimumValue, maximumValue + 1);
        array.Add(reusedValue);
        removalReference.Add(reusedValue);
        Assert.Equal(1, array.Count);
        Assert.Equal(capacityBeforeRemoval, array.Capacity);
        Assert.Equal(reusedValue, array[0]);
        Assert.Equal(removalReference, array);
        output.WriteLine($"Add({reusedValue}): Count={array.Count}, Capacity={array.Capacity}; массив снова используется.");

        // Act / Assert: ожидаемые ошибки аргументов.
        output.WriteLine("");
        output.WriteLine("8. Проверяем недопустимые аргументы");
        ArgumentOutOfRangeException capacityException = Assert.Throws<ArgumentOutOfRangeException>(
            "capacity", () => new DynamicArray<int>(0));
        output.WriteLine($"Вместимость 0: {capacityException.GetType().Name}, параметр '{capacityException.ParamName}'");

        ArgumentOutOfRangeException indexException = Assert.Throws<ArgumentOutOfRangeException>("index", () =>
        {
            _ = array[array.Count];
        });
        output.WriteLine($"Индекс Count ({array.Count}): {indexException.GetType().Name}, параметр '{indexException.ParamName}'");

        using IEnumerator<int> preservedIterator = array.GetEnumerator();
        Assert.True(preservedIterator.MoveNext());
        ArgumentOutOfRangeException removalException = Assert.Throws<ArgumentOutOfRangeException>(
            "index", () => array.RemoveAt(array.Count));
        Assert.Equal(reusedValue, preservedIterator.Current);
        Assert.False(preservedIterator.MoveNext());
        Assert.Equal(1, array.Count);
        Assert.Equal(capacityBeforeRemoval, array.Capacity);
        Assert.Equal(removalReference, array);
        output.WriteLine($"RemoveAt(Count): {removalException.GetType().Name}, параметр '{removalException.ParamName}'; содержимое и старый обход сохранились.");
        output.WriteLine("");
        // Act / Assert: поиск первого совпадения сохраняет обход.
        output.WriteLine("9. Ищем значения среди дубликатов");
        array.Add(reusedValue);
        using (IEnumerator<int> iterator = array.GetEnumerator())
        {
            Assert.True(iterator.MoveNext());
            int foundIndex = array.IndexOf((value, _) => value == reusedValue);
            bool found = array.Contains((value, _) => value == reusedValue);
            Assert.Equal(0, foundIndex);
            Assert.True(found);
            Assert.Equal(-1, array.IndexOf((value, _) => value == int.MaxValue));
            Assert.False(array.Contains((value, _) => value == int.MaxValue));
            Assert.Equal(reusedValue, iterator.Current);
            Assert.True(iterator.MoveNext());
            Assert.Equal(reusedValue, iterator.Current);
            Assert.False(iterator.MoveNext());
            output.WriteLine($"IndexOf({reusedValue})={foundIndex}, Contains={found}; отсутствующее значение={int.MaxValue}; обход сохранился.");
        }

        Assert.Equal(new[] { reusedValue, reusedValue }, array);
        Assert.Equal(2, array.Count);
        Assert.Equal(capacityBeforeRemoval, array.Capacity);
        int selectedPosition = array.IndexOf((_, position) => position == 1);
        Assert.Equal(1, selectedPosition);
        output.WriteLine($"Условие position == 1 выбрало индекс {selectedPosition} среди одинаковых значений.");
        InvalidOperationException searchMutation = Assert.Throws<InvalidOperationException>(() =>
        {
            array.IndexOf((value, position) =>
            {
                array[position] = value;
                return true;
            });
        });
        Assert.Equal(new[] { reusedValue, reusedValue }, array);
        output.WriteLine($"Запись из условия, даже того же значения: {searchMutation.GetType().Name}; поиск прерван.");

        // Act / Assert: очистка и повторное использование.
        output.WriteLine("");
        output.WriteLine("10. Очищаем массив с сохранением вместимости");
        using (IEnumerator<int> iterator = array.GetEnumerator())
        {
            array.Clear();
            Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
        }

        Assert.Equal(0, array.Count);
        Assert.Empty(array);
        Assert.Equal(capacityBeforeRemoval, array.Capacity);
        Assert.Equal(-1, array.IndexOf((value, _) => value == reusedValue));
        Assert.False(array.Contains((value, _) => value == reusedValue));
        output.WriteLine($"Clear(): Count={array.Count}, Capacity={array.Capacity}; прежнее значение отсутствует, старый обход инвалидирован.");
        using (IEnumerator<int> iterator = array.GetEnumerator())
        {
            array.Clear();
            Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
        }

        Assert.Equal(0, array.Count);
        Assert.Equal(capacityBeforeRemoval, array.Capacity);
        array.Add(reusedValue);
        Assert.Equal(reusedValue, Assert.Single(array));
        Assert.Equal(1, array.Count);
        Assert.Equal(capacityBeforeRemoval, array.Capacity);
        output.WriteLine($"Повторная очистка пустого массива инвалидировала обход; Add({reusedValue}) снова работает.");
        output.WriteLine("");
        output.WriteLine("Эксперимент завершён. Все проверки пройдены.");
    }
}
