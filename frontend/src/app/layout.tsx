import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "BARBERCLOUD | CRM",
  description: "Gestão inteligente para barbearias",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="pt-BR" className="dark">
      <body>{children}</body>
    </html>
  );
}
