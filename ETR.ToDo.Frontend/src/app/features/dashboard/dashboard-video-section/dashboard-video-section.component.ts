import { Component } from '@angular/core';
import { YouTubePlayer } from '@angular/youtube-player';

@Component({
  selector: 'app-dashboard-video-section',
  templateUrl: './dashboard-video-section.component.html',
  styleUrl: './dashboard-video-section.component.scss',
  imports: [YouTubePlayer],
})
export class DashboardVideoSectionComponent {}
