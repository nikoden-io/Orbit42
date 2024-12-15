import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Orbit 42",
  description: "Once upon a time in doubt galaxy...",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        {children}
      </body>
    </html>
  );
}
