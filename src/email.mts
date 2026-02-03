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
    
    // Strip out style tags from htmlBody
    const cleanHtmlBody = htmlBody.replace(/<style[^>]*>[\s\S]*?<\/style>/gi, '');

    const emailHtml = `
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
</head>
<body>
    <div>
        <p><span>Tokens Used:</span> ${tokenCount}</p>
        <p><span>Execution Time:</span> ${executionTimeSeconds}s</p>
        <p><span>Generated:</span> ${new Date().toISOString()}</p>
    </div>
    <div>
        ${cleanHtmlBody}
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
