import { Component, OnInit } from '@angular/core';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexYAxis,
  ApexStroke,
  ApexDataLabels,
  ApexMarkers,
  ApexTooltip,
  ApexGrid,
} from 'ng-apexcharts';
import { DashboardService, DashboardOverviewDto } from './dashboard.service';

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  yaxis: ApexYAxis;
  stroke: ApexStroke;
  markers: ApexMarkers;
  tooltip: ApexTooltip;
  grid: ApexGrid;
  colors: string[];
  dataLabels: ApexDataLabels;
};

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  loading = true;
  overview: DashboardOverviewDto | null = null;

  strengthChart: Partial<ChartOptions> = {};
  volumeChart: Partial<ChartOptions> = {};
  forTimeChart: Partial<ChartOptions> = {};

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getOverview().subscribe({
      next: (data) => {
        this.overview = data;
        this.buildCharts(data);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  private buildCharts(data: DashboardOverviewDto): void {
    const sharedGrid: ApexGrid = { borderColor: '#E4E6EF', strokeDashArray: 3 };
    const noDataLabels: ApexDataLabels = { enabled: false };

    this.strengthChart = {
      series: [{ name: '1RM', data: data.charts.strength1Rm.data }],
      chart: { type: 'line', height: 180, toolbar: { show: false } },
      xaxis: {
        categories: data.charts.strength1Rm.labels,
        labels: { style: { fontSize: '10px' } },
      },
      yaxis: { labels: { formatter: (v: number) => v + ' kg' } },
      stroke: { curve: 'smooth', width: 2 },
      markers: { size: 4, colors: ['#FF7A1A'] },
      tooltip: { x: { show: true } },
      grid: sharedGrid,
      colors: ['#FF7A1A'],
      dataLabels: noDataLabels,
    };

    this.volumeChart = {
      series: [{ name: 'kg', data: data.charts.weeklyVolume.data }],
      chart: { type: 'bar', height: 180, toolbar: { show: false } },
      xaxis: {
        categories: data.charts.weeklyVolume.labels,
        labels: { style: { fontSize: '10px' } },
      },
      yaxis: { labels: { formatter: (v: number) => v + ' kg' } },
      stroke: { width: 0 },
      markers: { size: 0 },
      tooltip: { x: { show: true } },
      grid: sharedGrid,
      colors: ['#009EF7'],
      dataLabels: noDataLabels,
    };

    this.forTimeChart = {
      series: [{ name: 'sec', data: data.charts.forTimeBest.data }],
      chart: { type: 'line', height: 180, toolbar: { show: false } },
      xaxis: {
        categories: data.charts.forTimeBest.labels,
        labels: { style: { fontSize: '10px' } },
      },
      yaxis: {
        reversed: true,
        labels: { formatter: (v: number) => this.formatSec(v) },
      },
      stroke: { curve: 'smooth', width: 2 },
      markers: { size: 4, colors: ['#50CD89'] },
      tooltip: { x: { show: true }, y: { formatter: (v: number) => this.formatSec(v) } },
      grid: sharedGrid,
      colors: ['#50CD89'],
      dataLabels: noDataLabels,
    };
  }

  formatSec(totalSec: number): string {
    const m = Math.floor(totalSec / 60);
    const s = totalSec % 60;
    return `${m}:${String(s).padStart(2, '0')}`;
  }

  wodBadgeClass(type: string): string {
    const map: Record<string, string> = {
      ForTime: 'badge-light-danger',
      AMRAP: 'badge-light-warning',
      EMOM: 'badge-light-info',
      Tabata: 'badge-light-primary',
    };
    return map[type] ?? 'badge-light-secondary';
  }

  recommendationIcon(code: string): string {
    const map: Record<string, string> = {
      'low-activity': 'ki-timer',
      'plateau': 'ki-graph-up',
      'consistency': 'ki-calendar',
    };
    return map[code] ?? 'ki-information';
  }

  recommendationColor(code: string): string {
    const map: Record<string, string> = {
      'low-activity': 'text-warning',
      'plateau': 'text-danger',
      'consistency': 'text-primary',
    };
    return map[code] ?? 'text-muted';
  }
}
