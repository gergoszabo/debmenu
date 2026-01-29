import nodemailer from 'nodemailer';

interface EmailConfig {
    smtpServer: string;
    smtpPort: number;
    senderUsername: string;
    senderPassword: string;
    recipientAddress: string;
}

export function getEmailConfig(): EmailConfig | null {
    const smtpServer = process.env.SMTP_SERVER;
    const smtpPort = process.env.SMTP_PORT;
    const senderUsername = process.env.SMTP_USERNAME;
    const senderPassword = process.env.SMTP_PASSWORD;
    const recipientAddress = process.env.SMTP_RECIPIENT;

    if (!smtpServer || !smtpPort || !senderUsername || !senderPassword || !recipientAddress) {
        console.log('Email config incomplete, skipping email sending');
        return null;
    }

    return {
        smtpServer,
        smtpPort: parseInt(smtpPort, 10),
        senderUsername,
        senderPassword,
        recipientAddress,
    };
}

export async function sendEmailSummary(
    tokenCount: number,
    htmlBody: string,
    executionTimeMs: number
): Promise<void> {
    const config = getEmailConfig();
    if (!config) {
        return;
    }

    const transporter = nodemailer.createTransport({
        host: config.smtpServer,
        port: config.smtpPort,
        secure: config.smtpPort === 465, // true for 465, false for other ports
        auth: {
            user: config.senderUsername,
            pass: config.senderPassword,
        },
    });

    const executionTimeSeconds = (executionTimeMs / 1000).toFixed(2);

    const emailHtml = `
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: system-ui, -apple-system, sans-serif; color: #333; line-height: 1.6; }
        .summary { background: #f5f5f5; padding: 12px; border-radius: 6px; margin-bottom: 20px; }
        .summary p { margin: 8px 0; }
        .label { font-weight: bold; }
        .content { background: white; padding: 20px; border: 1px solid #ddd; border-radius: 6px; }
    </style>
</head>
<body>
    <div class="summary">
        <p><span class="label">Tokens Used:</span> ${tokenCount}</p>
        <p><span class="label">Execution Time:</span> ${executionTimeSeconds}s</p>
        <p><span class="label">Generated:</span> ${new Date().toISOString()}</p>
    </div>
    <div class="content">
        ${htmlBody}
    </div>
</body>
</html>
    `.trim();

    await transporter.sendMail({
        from: config.senderUsername,
        to: config.recipientAddress,
        subject: `Daily Menu Generation Summary - ${new Date().toISOString().split('T')[0]}`,
        html: emailHtml,
    });

    console.log(`Email sent successfully to ${config.recipientAddress}`);
}
