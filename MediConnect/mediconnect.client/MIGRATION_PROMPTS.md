# MediConnect - React/TypeScript Migration Prompts

## 📋 Thông Tin Chung

**Dự án:** MediConnect - Healthcare Management Platform  
**Tech Stack:** React 18+ with TypeScript, Vite, TailwindCSS  
**Design System:** Material Symbols Icons, Manrope Font  
**Color Scheme:** Primary: #137fec, Background Light: #f6f7f8, Background Dark: #101922

---

## 🏗️ Cấu Trúc Thư Mục Chuẩn

```
mediconnect.client/
├── src/
│   ├── assets/              # Static files (images, fonts)
│   ├── components/          # Reusable UI components
│   │   ├── common/         # Buttons, Inputs, Cards, etc.
│   │   ├── layout/         # Header, Footer, Sidebar, Navigation
│   │   └── forms/          # Form components
│   ├── context/            # React Context & Global State
│   │   ├── AuthContext.tsx
│   │   ├── ThemeContext.tsx
│   │   └── NotificationContext.tsx
│   ├── data/               # Static data & models
│   │   ├── mockData.ts
│   │   └── types.ts
│   ├── features/           # Feature-based modules
│   │   ├── auth/          # Login, Register, Password Reset
│   │   ├── patient/       # Patient Dashboard, Profile, Records
│   │   ├── doctor/        # Doctor Dashboard, Schedule, Patients
│   │   ├── appointments/  # Booking, Management
│   │   ├── search/        # Doctor Search & Filters
│   │   └── admin/         # Admin Dashboard & Management
│   ├── hooks/             # Custom React Hooks
│   │   ├── useAuth.ts
│   │   ├── useTheme.ts
│   │   └── useDebounce.ts
│   ├── layouts/           # Page layouts
│   │   ├── MainLayout.tsx
│   │   ├── DashboardLayout.tsx
│   │   └── AuthLayout.tsx
│   ├── lib/              # External libraries & utilities
│   │   └── api.ts
│   ├── pages/            # Page components (Route level)
│   ├── services/         # API services
│   │   ├── authService.ts
│   │   ├── appointmentService.ts
│   │   └── doctorService.ts
│   ├── styles/           # Global styles
│   │   ├── globals.css
│   │   └── tailwind.css
│   └── utils/            # Utility functions
│       ├── formatters.ts
│       ├── validators.ts
│       └── helpers.ts
```

---

## 🎯 PROMPT 1: Authentication Pages

### 1.1 Unified Login Page

**File Location:** `src/features/auth/pages/LoginPage.tsx`

**Prompt:**

```
Create a modern, accessible login page for MediConnect healthcare platform using React TypeScript with the following specifications:

DESIGN REQUIREMENTS:
- Split-screen layout (Desktop): 60% illustration left, 40% form right
- Mobile-responsive: Stack vertically, form on top
- Color scheme: Primary #137fec, Background Light #f6f7f8, Background Dark #101922
- Dark mode support via Context API
- Font: Manrope (sans-serif)
- Icons: Material Symbols Outlined

FEATURES:
1. Role Selection Toggle:
   - Segmented button for Patient/Doctor selection
   - Active state with primary color background
   - Smooth transitions

2. Form Fields:
   - Email input with validation
   - Password input with show/hide toggle
   - Remember me checkbox
   - Forgot password link
   - Form validation with error messages

3. Authentication:
   - Primary login button with loading state
   - Social login options (Google, Apple)
   - Sign up redirect link

4. Left Side Illustration:
   - Gradient background with blur effects
   - Brand logo and tagline
   - Hero image with rounded corners and shadow
   - Responsive hide on mobile

5. Accessibility:
   - ARIA labels for all inputs
   - Keyboard navigation support
   - Focus states for all interactive elements
   - Screen reader friendly

DEPENDENCIES:
- react-hook-form for form management
- zod for validation schema
- react-router-dom for navigation
- Custom useAuth hook from hooks/useAuth.ts

COMPONENTS TO CREATE:
- LoginPage.tsx (main page)
- LoginForm.tsx (form component)
- RoleToggle.tsx (reusable toggle)
- SocialLoginButtons.tsx

STATE MANAGEMENT:
- Use AuthContext for authentication state
- Handle loading, error, and success states
- Redirect based on user role after login

Please implement with TypeScript interfaces, proper error handling, and responsive design using TailwindCSS utility classes.
```

---

### 1.2 Patient Registration Page

**File Location:** `src/features/auth/pages/PatientRegistrationPage.tsx`

**Prompt:**

````
Create a multi-step patient registration form for MediConnect using React TypeScript:

FEATURES:
1. Multi-Step Form (3 Steps):
   - Step 1: Contact Details (Name, Email, Phone)
   - Step 2: Personal Details (DOB, Gender, Address, Emergency Contact)
   - Step 3: Medical Information (Blood Type, Allergies, Medical History)

2. Progress Indicator:
   - Visual progress bar with percentage
   - Step counter (Step X of 3)
   - Next step preview text

3. Form Validation:
   - Real-time validation with react-hook-form
   - Email format validation
   - Phone number formatting
   - Required field indicators

4. Navigation:
   - Next/Previous buttons
   - Disabled state when validation fails
   - Final submit button on last step

5. UI Elements:
   - Consistent input styling with TailwindCSS
   - Dark mode support
   - Loading states during submission
   - Success confirmation modal

COMPONENTS:
- PatientRegistrationPage.tsx (container)
- RegistrationStep1.tsx (contact info)
- RegistrationStep2.tsx (personal details)
- RegistrationStep3.tsx (medical info)
- ProgressBar.tsx (reusable)
- FormInput.tsx (reusable input component)

TYPES:
```typescript
interface PatientRegistrationData {
  // Step 1
  fullName: string;
  email: string;
  phone: string;
  password: string;

  // Step 2
  dateOfBirth: string;
  gender: 'male' | 'female' | 'other';
  address: string;
  city: string;
  zipCode: string;
  emergencyContact: string;
  emergencyPhone: string;

  // Step 3
  bloodType: string;
  allergies: string[];
  medications: string[];
  medicalConditions: string[];
  insuranceProvider?: string;
  insuranceId?: string;
}
````

Implement with proper TypeScript typing, form validation, responsive design, and accessibility features.

```

---

### 1.3 Password Reset Page

**File Location:** `src/features/auth/pages/PasswordResetPage.tsx`

**Prompt:**
```

Create a password reset flow with two pages:

PAGE 1: Request Reset

- Email input field
- Submit button
- Back to login link
- Loading state during API call
- Success message with instructions

PAGE 2: Reset Password

- New password input
- Confirm password input
- Password strength indicator
- Token validation from URL params
- Submit button with loading state
- Success redirect to login

VALIDATION:

- Email format validation
- Password strength requirements (min 8 chars, uppercase, lowercase, number, special char)
- Password match validation
- Expired token handling

COMPONENTS:

- PasswordResetRequestPage.tsx
- PasswordResetPage.tsx
- PasswordStrengthIndicator.tsx
- SuccessMessage.tsx

Implement with proper error handling, loading states, and user feedback.

```

---

## 🎯 PROMPT 2: Patient Features

### 2.1 Patient Dashboard

**File Location:** `src/features/patient/pages/PatientDashboard.tsx`

**Prompt:**
```

Create a comprehensive patient dashboard for MediConnect:

LAYOUT:

- Sidebar navigation (fixed on desktop, hamburger on mobile)
- Main content area with greeting and date
- Card-based layout for different sections

SIDEBAR COMPONENTS:

- User profile card (avatar, name, patient ID)
- Navigation links with icons:
  - Home (active)
  - Search Doctors
  - My Appointments
  - Medical Records
  - Settings
- Bottom section: Help Center, Logout

DASHBOARD SECTIONS:

1. Welcome Header:

   - Personalized greeting
   - Current date
   - Quick action buttons (Book Appointment, Emergency)

2. Upcoming Appointments Card:

   - List of next 3 appointments
   - Doctor info, date, time, specialty
   - Quick actions: Reschedule, Cancel, Join Video
   - Empty state if no appointments

3. Health Summary Cards:

   - Recent vitals (Blood Pressure, Heart Rate, Weight)
   - Mini charts or indicators
   - Last updated timestamp

4. Quick Actions Grid:

   - Search Doctors
   - View Records
   - Prescriptions
   - Lab Results
   - Each with icon and count badge

5. Recent Activity Timeline:

   - Last 5 activities
   - Icons for different activity types
   - Relative timestamps

6. Recommended Doctors:
   - Horizontal scrollable list
   - Doctor cards with specialty, rating
   - Quick book button

COMPONENTS TO CREATE:

- PatientDashboard.tsx
- PatientSidebar.tsx
- UpcomingAppointmentsCard.tsx
- HealthSummaryCard.tsx
- QuickActionsGrid.tsx
- ActivityTimeline.tsx
- RecommendedDoctors.tsx

STATE MANAGEMENT:

- Fetch user data on mount
- Real-time appointment updates
- Loading skeletons for async data

RESPONSIVE DESIGN:

- Desktop: Sidebar always visible
- Tablet: Collapsible sidebar
- Mobile: Hamburger menu

Implement with TypeScript, proper data fetching, error boundaries, and loading states.

```

---

### 2.2 Search & Filter Doctors

**File Location:** `src/features/search/pages/DoctorSearchPage.tsx`

**Prompt:**
```

Create an advanced doctor search and filter system:

LAYOUT:

- Header with search bar and notifications
- Left sidebar filters (sticky)
- Main content area with doctor cards
- Right sidebar for featured doctors (optional)

SEARCH FEATURES:

1. Global Search Bar:

   - Autocomplete suggestions
   - Search by name, specialty, clinic
   - Recent searches
   - Debounced input (300ms)

2. Filter Sidebar:

   - Specialty checkboxes (Cardiology, Pediatrics, Dermatology, etc.)
   - Availability buttons (Today, Tomorrow, This Week)
   - Gender filter
   - Languages spoken
   - Insurance accepted
   - Consultation type (In-person, Video, Chat)
   - Rating filter (4+ stars)
   - Price range slider
   - Reset all filters button

3. Sort Options:

   - Relevance (default)
   - Highest rated
   - Most reviewed
   - Nearest location
   - Soonest available
   - Price: Low to High

4. Doctor Cards Grid:

   - Doctor photo with verified badge
   - Name, credentials, specialty
   - Rating stars and review count
   - Years of experience
   - Available slots preview
   - Location with distance
   - Price per consultation
   - Quick actions: View Profile, Book Now

5. Pagination:
   - Load more button or infinite scroll
   - Results count display

COMPONENTS:

- DoctorSearchPage.tsx
- SearchBar.tsx (with autocomplete)
- FilterSidebar.tsx
- SpecialtyFilter.tsx
- AvailabilityFilter.tsx
- DoctorCard.tsx
- SortDropdown.tsx
- PaginationControls.tsx

STATE MANAGEMENT:

```typescript
interface SearchState {
  query: string;
  filters: {
    specialties: string[];
    availability: string[];
    gender: string[];
    languages: string[];
    insurance: string[];
    consultationType: string[];
    minRating: number;
    priceRange: [number, number];
  };
  sortBy: string;
  page: number;
  results: Doctor[];
  totalResults: number;
  isLoading: boolean;
}
```

HOOKS:

- useDebounce for search input
- useQuery for data fetching (React Query)
- useLocalStorage for saved filters

Implement with proper URL state management (query params), responsive design, and performance optimization.

```

---

### 2.3 Appointment Management

**File Location:** `src/features/appointments/pages/AppointmentManagerPage.tsx`

**Prompt:**
```

Create a comprehensive appointment management system:

FEATURES:

1. Tabs Navigation:

   - Upcoming (with count badge)
   - Past Appointments
   - Cancelled

2. Appointment Cards (Upcoming):

   - Doctor info with avatar
   - Date, time, duration
   - Appointment type badge (In-person/Video)
   - Location or meeting link
   - Status indicator
   - Action buttons:
     - Reschedule
     - Cancel
     - Add to Calendar
     - Join Video (if applicable and time is near)

3. Filters & Search:

   - Search by doctor name
   - Filter by specialty
   - Date range picker
   - Sort by date

4. Empty States:

   - No upcoming appointments
   - Illustration with CTA button

5. Appointment Details Modal:

   - Full appointment information
   - Doctor profile summary
   - Patient notes
   - Attached documents
   - Preparation instructions

6. Booking Flow:
   - Quick book new appointment button
   - Opens booking wizard

COMPONENTS:

- AppointmentManagerPage.tsx
- AppointmentTabs.tsx
- AppointmentCard.tsx
- AppointmentFilters.tsx
- AppointmentDetailsModal.tsx
- CancelAppointmentModal.tsx (with reason selection)
- RescheduleModal.tsx
- EmptyState.tsx

TYPES:

```typescript
interface Appointment {
  id: string;
  doctorId: string;
  doctorName: string;
  doctorAvatar: string;
  specialty: string;
  date: string;
  time: string;
  duration: number;
  type: "in-person" | "video" | "chat";
  status: "scheduled" | "completed" | "cancelled" | "no-show";
  location?: string;
  meetingLink?: string;
  notes?: string;
  price: number;
}
```

Implement with proper state management, API integration, real-time updates, and notification system.

```

---

## 🎯 PROMPT 3: Doctor Features

### 3.1 Doctor Dashboard

**File Location:** `src/features/doctor/pages/DoctorDashboard.tsx`

**Prompt:**
```

Create a professional doctor dashboard:

SIDEBAR:

- Doctor profile (avatar, name, specialty)
- Navigation:
  - Schedule (active)
  - Patients
  - Performance/Analytics
  - Settings
- New Appointment button (CTA)

DASHBOARD LAYOUT:

1. Top Bar:

   - Breadcrumbs
   - Welcome message
   - Search patients
   - Notifications
   - Profile dropdown

2. Today's Overview Cards:

   - Total Appointments Today
   - Completed Appointments
   - Pending Appointments
   - Total Revenue Today
   - Each with icon and comparison to yesterday

3. Today's Schedule Timeline:

   - Vertical timeline view
   - Time slots (8 AM - 8 PM)
   - Appointment blocks with:
     - Patient name and photo
     - Appointment type
     - Duration
     - Quick actions (Start, Cancel, Reschedule)
   - Empty slots
   - Current time indicator

4. Upcoming Patients List:

   - Next 5 patients
   - Patient info preview
   - Reason for visit
   - Previous visit history link
   - Prepare button

5. Quick Stats Grid:

   - Patients Treated This Month
   - Average Rating
   - Response Time
   - Consultation Hours

6. Recent Patient Reviews:
   - Last 3 reviews with ratings
   - Patient name and date
   - Review text
   - Reply option

COMPONENTS:

- DoctorDashboard.tsx
- DoctorSidebar.tsx
- StatsCard.tsx
- ScheduleTimeline.tsx
- TimeSlot.tsx
- UpcomingPatientsList.tsx
- PatientReviewCard.tsx

FUNCTIONALITY:

- Real-time schedule updates
- Drag and drop to reschedule
- Quick patient notes
- One-click video consultation start

Implement with real-time WebSocket updates, responsive design, and performance optimization.

```

---

### 3.2 Doctor Profile & Booking

**File Location:** `src/features/doctor/pages/DoctorProfilePage.tsx`

**Prompt:**
```

Create a detailed doctor profile page with booking functionality:

LAYOUT SECTIONS:

1. Profile Header Card:

   - Large doctor photo with verified badge
   - Name, credentials (MD, PhD)
   - Specialty
   - Location with map icon
   - Years of experience
   - Rating stars with review count
   - Languages spoken
   - Action buttons: Book Appointment, Message, Save

2. About Section:

   - Bio/Description
   - Education (university, degree, year)
   - Certifications
   - Professional memberships
   - Awards and recognitions

3. Specialties & Services:

   - List of conditions treated
   - Services offered
   - Treatment approaches

4. Experience & Expertise:

   - Years in practice
   - Patients treated
   - Success stories (if applicable)

5. Insurance & Pricing:

   - Accepted insurance providers
   - Consultation fees
   - Payment methods

6. Office Information:

   - Clinic address with map
   - Contact information
   - Office hours
   - Facilities available

7. Patient Reviews Section:

   - Average rating breakdown (5, 4, 3, 2, 1 stars with bars)
   - Review filters (Most Recent, Highest Rated, Verified Patients)
   - Individual review cards:
     - Patient name (anonymized option)
     - Rating stars
     - Date
     - Review text
     - Doctor response (if any)
   - Pagination

8. Sticky Booking Widget (Right Sidebar):
   - Calendar date picker
   - Available time slots
   - Consultation type selector
   - Price display
   - Book Now button
   - Stays visible on scroll (sticky)

BOOKING FLOW:

1. Select date from calendar (highlighted available dates)
2. Show available time slots for selected date
3. Select consultation type (in-person/video)
4. Click Book Now
5. Open booking confirmation modal:
   - Appointment summary
   - Patient details form
   - Reason for visit
   - Payment method
   - Terms acceptance
   - Confirm booking button

COMPONENTS:

- DoctorProfilePage.tsx
- ProfileHeader.tsx
- AboutSection.tsx
- SpecialtiesSection.tsx
- ReviewsSection.tsx
- ReviewCard.tsx
- BookingWidget.tsx (sticky)
- DateTimePicker.tsx
- BookingConfirmationModal.tsx

TYPES:

```typescript
interface Doctor {
  id: string;
  name: string;
  credentials: string[];
  specialty: string;
  avatar: string;
  rating: number;
  reviewCount: number;
  yearsExperience: number;
  languages: string[];
  bio: string;
  education: Education[];
  certifications: string[];
  insuranceAccepted: string[];
  consultationFee: number;
  clinicAddress: string;
  availability: Availability[];
}
```

Implement with calendar integration, real-time availability, payment processing, and confirmation emails.

```

---

## 🎯 PROMPT 4: Admin Features

### 4.1 Admin Dashboard

**File Location:** `src/features/admin/pages/AdminDashboard.tsx`

**Prompt:**
```

Create a comprehensive admin dashboard with analytics:

SIDEBAR NAVIGATION:

- Admin profile
- Dashboard (active)
- User Management
- Doctor Management
- Appointment Management
- Specialties & Departments
- Reports & Analytics
- System Logs
- Settings

DASHBOARD SECTIONS:

1. Key Metrics Cards (4 cards in row):

   - Total Users (with growth percentage)
   - Active Doctors
   - Appointments Today
   - Total Revenue
   - Each with icon, value, and trend indicator

2. Charts Section:

   - Appointments Over Time (Line chart - 30 days)
   - Revenue by Month (Bar chart)
   - Popular Specialties (Pie chart)
   - User Growth (Area chart)

3. Recent Activity Feed:

   - Real-time updates
   - User registrations
   - New doctor applications
   - Appointments booked
   - System events
   - Timestamps

4. Pending Actions Table:

   - Doctor verification requests
   - Reported issues
   - Refund requests
   - Priority level indicators
   - Quick action buttons

5. System Health Indicators:

   - Server status
   - Database status
   - API response time
   - Active sessions
   - Color-coded status badges

6. Quick Actions:
   - Add New Doctor
   - Create Announcement
   - Export Reports
   - Backup Database

COMPONENTS:

- AdminDashboard.tsx
- AdminSidebar.tsx
- MetricCard.tsx
- AppointmentChart.tsx (using recharts)
- RevenueChart.tsx
- ActivityFeed.tsx
- PendingActionsTable.tsx
- SystemHealthPanel.tsx

LIBRARIES:

- recharts for charts
- date-fns for date formatting
- react-table for tables

Implement with real-time data updates, export functionality, and responsive design.

```

---

### 4.2 User Management

**File Location:** `src/features/admin/pages/UserManagementPage.tsx`

**Prompt:**
```

Create a user management system:

FEATURES:

1. Data Table with columns:

   - Avatar & Name
   - Email
   - Role (Patient/Doctor/Admin)
   - Status (Active/Inactive/Suspended)
   - Registration Date
   - Last Login
   - Actions (View, Edit, Suspend, Delete)

2. Filters & Search:

   - Search by name or email
   - Filter by role
   - Filter by status
   - Date range filter
   - Advanced filters toggle

3. Bulk Actions:

   - Select multiple users
   - Bulk export
   - Bulk status change
   - Bulk delete (with confirmation)

4. User Details Modal:

   - Full user information
   - Activity history
   - Appointments history
   - Notes section
   - Status change controls

5. Add/Edit User Form:
   - Create new user
   - Edit existing user
   - Role assignment
   - Permission settings

COMPONENTS:

- UserManagementPage.tsx
- UsersTable.tsx (with sorting, filtering)
- UserFilters.tsx
- UserDetailsModal.tsx
- UserFormModal.tsx
- BulkActionsBar.tsx

Implement with pagination, sorting, CSV export, and proper access control.

```

---

## 🎯 PROMPT 5: Additional Features

### 5.1 Medical Records Page

**File Location:** `src/features/patient/pages/MedicalRecordsPage.tsx`

**Prompt:**
```

Create a comprehensive medical records management system:

FEATURES:

- Document categories (Lab Results, Prescriptions, Imaging, Reports)
- Upload new documents
- Download/Print documents
- Share with doctors
- Search and filter
- Timeline view of medical history

Implement with file upload, preview modal, and secure document handling.

```

---

### 5.2 Schedule Management (Doctor)

**File Location:** `src/features/doctor/pages/ScheduleManagementPage.tsx`

**Prompt:**
```

Create a schedule management system for doctors:

FEATURES:

- Weekly/Monthly calendar view
- Set availability hours
- Block time slots
- Recurring schedules
- Time-off management
- View booked appointments
- Drag-and-drop rescheduling

Use FullCalendar or react-big-calendar library.

```

---

### 5.3 AI Health Triage Assistant

**File Location:** `src/features/ai-triage/pages/AITriagePage.tsx`

**Prompt:**
```

Create an AI-powered health triage chat interface:

FEATURES:

- Chat interface with message bubbles
- Symptom checker flow
- Multiple choice questions
- Severity assessment
- Specialist recommendations
- Emergency warning system
- Save chat history

Implement with chat UI library and WebSocket for real-time responses.

```

---

### 5.4 Statistical Reports

**File Location:** `src/features/admin/pages/ReportsPage.tsx`

**Prompt:**
```

Create a comprehensive reporting system:

FEATURES:

- Report templates (Appointments, Revenue, User Growth, Doctor Performance)
- Date range selector
- Export to PDF/Excel
- Interactive charts
- Comparison views
- Scheduled reports

Use recharts for visualizations and jsPDF for PDF generation.

```

---

## 🛠️ Common Components Library

### Prompt for Component Library

```

Create a reusable component library in src/components/common/:

COMPONENTS TO CREATE:

1. Button.tsx

   - Variants: primary, secondary, outline, ghost, danger
   - Sizes: sm, md, lg
   - Loading state with spinner
   - Disabled state
   - Icon support

2. Input.tsx

   - Text, email, password, number types
   - Label and error message
   - Helper text
   - Prefix/suffix icons
   - Validation states

3. Select.tsx

   - Single and multi-select
   - Searchable option
   - Custom option rendering
   - Grouped options

4. Modal.tsx

   - Flexible content
   - Close on backdrop click
   - Sizes: sm, md, lg, full
   - Animation support

5. Card.tsx

   - Header, body, footer sections
   - Hover effects
   - Shadow variants

6. Badge.tsx

   - Status badges with colors
   - Sizes and variants

7. Avatar.tsx

   - Image with fallback initials
   - Sizes and shapes
   - Status indicator

8. Toast.tsx

   - Success, error, warning, info types
   - Auto-dismiss
   - Action buttons

9. Spinner.tsx

   - Multiple sizes
   - Different styles

10. Tooltip.tsx
    - Positioning options
    - Delay settings

Each component should:

- Be fully typed with TypeScript
- Support dark mode
- Be accessible (ARIA labels)
- Have proper documentation
- Include Storybook stories

````

---

## 🔧 Utilities & Hooks

### Custom Hooks to Create

```typescript
// src/hooks/useAuth.ts
export function useAuth() {
  // Authentication state and methods
}

// src/hooks/useTheme.ts
export function useTheme() {
  // Theme switching (light/dark)
}

// src/hooks/useDebounce.ts
export function useDebounce<T>(value: T, delay: number): T {
  // Debounce any value
}

// src/hooks/useLocalStorage.ts
export function useLocalStorage<T>(key: string, initialValue: T) {
  // Persistent state in localStorage
}

// src/hooks/useMediaQuery.ts
export function useMediaQuery(query: string): boolean {
  // Responsive breakpoints
}

// src/hooks/useAsync.ts
export function useAsync<T>(asyncFunction: () => Promise<T>) {
  // Handle async operations with loading/error states
}

// src/hooks/useForm.ts
export function useForm<T>(initialValues: T) {
  // Form state management
}
````

---

## 🎨 TailwindCSS Configuration

```javascript
// tailwind.config.js
export default {
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: "#137fec",
          50: "#e6f2ff",
          100: "#b3d9ff",
          // ... other shades
        },
        background: {
          light: "#f6f7f8",
          dark: "#101922",
        },
      },
      fontFamily: {
        display: ["Manrope", "sans-serif"],
      },
      borderRadius: {
        DEFAULT: "0.25rem",
        lg: "0.5rem",
        xl: "0.75rem",
      },
    },
  },
  plugins: [require("@tailwindcss/forms"), require("@tailwindcss/typography")],
};
```

---

## 🚀 Implementation Order

### Phase 1: Foundation

1. Setup project structure
2. Configure TailwindCSS and theme
3. Create common components library
4. Setup routing (React Router)
5. Create layout components
6. Implement AuthContext and ThemeContext

### Phase 2: Authentication

1. Login page
2. Patient registration
3. Doctor registration (if needed)
4. Password reset flow

### Phase 3: Patient Features

1. Patient dashboard
2. Search & filter doctors
3. Doctor profile & booking
4. Appointment management
5. Medical records

### Phase 4: Doctor Features

1. Doctor dashboard
2. Schedule management
3. Patient list management
4. Performance analytics

### Phase 5: Admin Features

1. Admin dashboard
2. User management
3. Doctor management
4. Specialties management
5. Reports & analytics
6. System logs

### Phase 6: Advanced Features

1. AI Health Triage
2. Video consultation integration
3. Real-time notifications
4. Chat system
5. Document management

---

## 📦 Key Dependencies

```json
{
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.20.0",
    "react-hook-form": "^7.48.0",
    "zod": "^3.22.0",
    "@tanstack/react-query": "^5.12.0",
    "axios": "^1.6.0",
    "date-fns": "^3.0.0",
    "recharts": "^2.10.0",
    "react-big-calendar": "^1.8.0",
    "lucide-react": "^0.294.0",
    "clsx": "^2.0.0",
    "tailwind-merge": "^2.1.0"
  },
  "devDependencies": {
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "typescript": "^5.3.0",
    "vite": "^5.0.0",
    "@vitejs/plugin-react": "^4.2.0",
    "tailwindcss": "^3.3.0",
    "autoprefixer": "^10.4.0",
    "postcss": "^8.4.0",
    "eslint": "^8.55.0",
    "prettier": "^3.1.0"
  }
}
```

---

## ✅ Best Practices

1. **TypeScript**: Always use proper typing, avoid `any`
2. **Component Organization**: One component per file
3. **Naming Conventions**:
   - Components: PascalCase
   - Files: PascalCase for components, camelCase for utilities
   - Functions: camelCase
   - Constants: UPPER_SNAKE_CASE
4. **State Management**: Use Context API for global state, local state for component-specific
5. **API Calls**: Always use service layer, never direct API calls in components
6. **Error Handling**: Implement error boundaries and proper error UI
7. **Loading States**: Always show loading indicators during async operations
8. **Accessibility**: ARIA labels, keyboard navigation, focus management
9. **Performance**: Code splitting, lazy loading, memoization where needed
10. **Testing**: Write unit tests for utilities, integration tests for features

---

## 🎓 Notes for AI Code Generation

When using these prompts:

- Start with foundation components first
- Build incrementally, one feature at a time
- Test each component before moving to the next
- Ensure dark mode works for all components
- Maintain consistent styling across all pages
- Reuse components wherever possible
- Keep accessibility in mind
- Document complex logic
- Use TypeScript strictly
- Follow the folder structure precisely

---

**Generated on:** January 15, 2026  
**Project:** MediConnect Healthcare Platform  
**Version:** 1.0.0
