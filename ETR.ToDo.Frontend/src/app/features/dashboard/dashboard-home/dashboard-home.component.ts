import { Component } from '@angular/core';
import { DashboardHeroSectionComponent } from '../dashboard-hero-section/dashboard-hero-section.component';
import { DashboardVideoSectionComponent } from '../dashboard-video-section/dashboard-video-section.component';
import { DashboardFeaturesSectionComponent } from '../dashboard-features-section/dashboard-features-section.component';
import { DashboardTestimonialsSectionComponent } from '../dashboard-testimonials-section/dashboard-testimonials-section.component';
import { DashboardPricingSectionComponent } from '../dashboard-pricing-section/dashboard-pricing-section.component';
import { DashboardCtaSectionComponent } from '../dashboard-cta-section/dashboard-cta-section.component';
import { DashboardFooterComponent } from '../dashboard-footer/dashboard-footer.component';
import { ScrollAnimationDirective } from '../../../shared/directives/scroll-animation.directive';

@Component({
  selector: 'app-dashboard-home',
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.scss',
  imports: [
    DashboardHeroSectionComponent,
    DashboardVideoSectionComponent,
    DashboardFeaturesSectionComponent,
    DashboardTestimonialsSectionComponent,
    DashboardPricingSectionComponent,
    DashboardCtaSectionComponent,
    DashboardFooterComponent,
    ScrollAnimationDirective,
  ],
})
export class DashboardHomeComponent {}
