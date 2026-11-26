namespace TeduMarket.Application.Notification;

/// <summary>Một email cần gửi; <paramref name="HtmlBody"/> và <paramref name="TextBody"/> là hai bản của cùng nội dung.</summary>
public sealed record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);
