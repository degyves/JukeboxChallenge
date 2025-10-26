import { fontFamily } from "tailwindcss/defaultTheme";

/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        brand: {
          50: "#EEF2FF",
          100: "#E0E7FF",
          200: "#C7D2FE",
          300: "#A5B4FC",
          400: "#818CF8",
          500: "#6366F1",
          600: "#4F46E5",
          700: "#4338CA",
          800: "#3730A3",
          900: "#312E81"
        },
        accent: {
          100: "#DCF9FF",
          200: "#BAF2FF",
          300: "#6EE7F9",
          400: "#22D3EE",
          500: "#0EA5E9",
          600: "#0284C7"
        },
        surface: {
          950: "#020617",
          900: "#0F172A",
          800: "#1E293B",
          700: "#273449",
          600: "#2E3A52"
        }
      },
      fontFamily: {
        heading: ["'Plus Jakarta Sans'", ...fontFamily.sans],
        body: ["'Manrope'", ...fontFamily.sans]
      },
      boxShadow: {
        glass: "0 35px 90px -45px rgba(14, 165, 233, 0.35)",
        "glass-soft": "0 20px 60px -35px rgba(99, 102, 241, 0.45)"
      },
      borderRadius: {
        xl: "1.5rem",
        "2xl": "2rem"
      },
      backdropBlur: {
        xs: "2px",
        md: "18px"
      }
    }
  },
  plugins: []
};
