/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./wwwroot/js/**/*.js"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', '-apple-system', 'BlinkMacSystemFont', '"Segoe UI"', 'Roboto', 'sans-serif'],
      },
      colors: {
        brand: {
          50: '#F8FAFC',
          100: '#F1F5F9',
          200: '#E2E8F0',
          300: '#CBD5E1',
          400: '#94A3B8',
          500: '#64748B',
          600: '#475569',
          700: '#334155',
          800: '#1E293B',
          900: '#111827',
          950: '#0B0F17',
        },
        status: {
          submitted: {
            bg: '#F1F5F9',
            text: '#334155',
            border: '#CBD5E1',
          },
          underreview: {
            bg: '#FEF3C7',
            text: '#92400E',
            border: '#FCD34D',
          },
          resolved: {
            bg: '#ECFDF5',
            text: '#065F46',
            border: '#A7F3D0',
          },
          closed: {
            bg: '#EEF2FF',
            text: '#3730A3',
            border: '#C7D2FE',
          }
        }
      }
    },
  },
  plugins: [],
}
