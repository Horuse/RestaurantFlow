namespace RestaurantFlow.Shared.Enums;

public enum OrderStatus
{
    Pending,      // В черзі
    InProgress,   // Готується
    Ready,        // Готово до видачі
    Completed,    // Видано
    Cancelled     // Скасовано
}