namespace Notenokand.Domain.Enums;

public enum BuildingStatus { Inactive = 0, Active = 1 }
public enum DailyLogType { General = 0, Environment = 1, Cleaning = 2, Harvest = 3, Expense = 4, Maintenance = 5 }
public enum HarvestStatus { Draft = 0, Confirmed = 1, Closed = 2 }
public enum SaleStatus { Draft = 0, Confirmed = 1, Cancelled = 2 }
public enum PaymentStatus { Unpaid = 0, PartiallyPaid = 1, Paid = 2, Overdue = 3 }
public enum TransactionType { Income = 1, Expense = 2 }
public enum MaintenanceType { Corrective = 1, Preventive = 2 }
public enum MaintenancePriority { Low = 1, Normal = 2, High = 3, Urgent = 4 }
public enum MaintenanceStatus { Reported = 1, InProgress = 2, AwaitingAcceptance = 3, Completed = 4, Cancelled = 5 }
public enum ImageKind { General = 0, Before = 1, After = 2, Lot = 3, Sample = 4, Document = 5 }
public enum QualityTrend { Worse = -1, Stable = 0, Better = 1, InsufficientData = 2 }
