<#macro emailLayout>
<!doctype html>
<html lang="${locale.language}" dir="${(ltr)?then('ltr','rtl')}">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="color-scheme" content="light dark">
    <title>${realmName}</title>
    <style>
        .content p {
            margin: 0 0 14px;
        }

        .content a {
            color: #0a84ff;
            font-weight: 600;
            text-decoration: none;
        }

        .content p:has(a:only-child) a {
            display: inline-block;
            padding: 11px 18px;
            color: #ffffff;
            background: #0a84ff;
            border-radius: 10px;
        }

        @media (prefers-color-scheme: dark) {
            .page { background: #121214 !important; }
            .card { background: #2c2c2e !important; border-color: #3a3a3c !important; }
            .title { color: #ffffff !important; }
            .content, .footer { color: #aeaeb2 !important; }
        }
    </style>
</head>
<body style="margin: 0; padding: 0;">
<table role="presentation" class="page" width="100%" cellpadding="0" cellspacing="0" border="0"
       style="background: #f4f5f7; padding: 32px 16px; font-family: Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;">
    <tr>
        <td align="center">
            <table role="presentation" class="card" width="100%" cellpadding="0" cellspacing="0" border="0"
                   style="max-width: 460px; background: #ffffff; border: 1px solid #d9dce3; border-radius: 20px; padding: 32px;">
                <tr>
                    <td align="center" style="padding-bottom: 20px;">
                        <table role="presentation" cellpadding="0" cellspacing="0" border="0">
                            <tr>
                                <td style="width: 44px; height: 44px; background: #0a84ff; border-radius: 12px; color: #ffffff; font-size: 24px; font-weight: 700; text-align: center; line-height: 44px;">C</td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td class="title" align="center"
                        style="padding-bottom: 18px; color: #1a1d24; font-size: 20px; font-weight: 700; letter-spacing: -0.02em;">
                        ${realmName}
                    </td>
                </tr>
                <tr>
                    <td class="content" style="color: #626875; font-size: 15px; line-height: 1.6;">
                        <#nested>
                    </td>
                </tr>
            </table>
            <p class="footer" style="margin: 16px 0 0; color: #626875; font-size: 12px;">
                You received this email because of your ${realmName} account.
            </p>
        </td>
    </tr>
</table>
</body>
</html>
</#macro>
