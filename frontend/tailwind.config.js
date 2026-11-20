/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        muted: 'var(--muted)',
        brand: {
          DEFAULT: '#C8006E',
          hover: '#A30059',
          soft: '#FCE9F3'
        },
        ink: {
          DEFAULT: '#1B1420',
          secondary: '#3D3542',
          muted: '#6F6772'
        },
        line: {
          DEFAULT: '#EAE6EC',
          strong: '#D9D2DD'
        },
        surface: '#FAF9FB',
        success: {
          DEFAULT: '#12794A',
          soft: '#E6F5EE'
        }
      },
      fontFamily: {
        sans: ['Manrope', 'system-ui', 'sans-serif']
      },
      borderRadius: {
        card: '14px'
      }
    }
  },
  plugins: []
};
