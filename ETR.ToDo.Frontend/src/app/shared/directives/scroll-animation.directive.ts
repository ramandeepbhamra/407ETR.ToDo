import { Directive, ElementRef, inject, input, OnInit } from '@angular/core';

@Directive({
  selector: '[scrollAnimation]',
})
export class ScrollAnimationDirective implements OnInit {
  private readonly element = inject(ElementRef);

  readonly animationClass = input('fade-in-up');
  readonly threshold = input(0.2);
  readonly delay = input(0);

  ngOnInit() {
    this.element.nativeElement.style.opacity = '0';
    this.element.nativeElement.style.transition = 'all 0.8s ease';
    this.element.nativeElement.style.transitionDelay = `${this.delay()}ms`;

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            this.element.nativeElement.style.opacity = '1';
            this.element.nativeElement.classList.add(this.animationClass());
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: this.threshold() }
    );

    observer.observe(this.element.nativeElement);
  }
}
