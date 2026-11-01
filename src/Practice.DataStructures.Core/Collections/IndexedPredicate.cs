namespace Practice.DataStructures.Core.Collections;

/// <summary>Проверяет условие для элемента и его позиции в упорядоченной коллекции.</summary>
/// <typeparam name="T">Тип проверяемого элемента.</typeparam>
/// <param name="item">Проверяемый элемент, включая допустимое для типа значение по умолчанию.</param>
/// <param name="index">Текущая позиция элемента, начиная с нуля.</param>
/// <returns><see langword="true"/>, если условие выполнено; иначе <see langword="false"/>.</returns>
public delegate bool IndexedPredicate<in T>(T item, int index);
